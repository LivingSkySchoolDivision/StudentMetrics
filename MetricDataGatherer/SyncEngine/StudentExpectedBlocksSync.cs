using LSKYStudentMetrics;
using LSKYStudentMetrics.Repositories.SchoolLogic;
using LSKYStudentMetrics.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.SyncEngine
{
    class StudentExpectedBlocksSync
    {
        public static void Sync(ConfigFile configFile, LogDelegate Log)
        {
            ConfigFileSyncPermissionsSection config = configFile.ExpectedAttendancePermissions;

            Log("========= EXPECTED ATTENDANCE ========= ");
            if (!config.AllowSync)
            {
                Log("This sync module is disabled in config file - skipping");
                return;
            }

            // Load all students that have enrolled classes
            //  - a DISTINCT() on the enrollment table should do nicely here

            // Load all of those student's schedules
            // - Load each class's schedule, then combine into the student's own schedule

            // Parse the school year from the config file, we'll need it later            
            InternalSchoolYearRepository _schoolYearRepo = new InternalSchoolYearRepository(configFile.DatabaseConnectionString_Internal);
            SchoolYear schoolYear = _schoolYearRepo.Get(configFile.SchoolYearName);
            if (schoolYear == null)
            {
                throw new InvalidSchoolYearException("School year from config file is invalid");
            }

            SLStudentRepository _studentRepo = new SLStudentRepository(configFile.DatabaseConnectionString_SchoolLogic);
            SLTrackRepository _trackRepo = new SLTrackRepository(configFile.DatabaseConnectionString_SchoolLogic);

            List<Student> dailyAttendanceStudents = _studentRepo.GetDailyAttendanceStudents();
            List<Student> periodAttendanceStudents = _studentRepo.GetPeriodAttendanceStudents();

            Log("Found " + dailyAttendanceStudents.Count() + " students in a daily track");
            Log("Found " + periodAttendanceStudents.Count() + " students in a period track");
            
            List<StudentExpectedAttendanceEntry> externalObjects = new List<StudentExpectedAttendanceEntry>();
            
            foreach (Student student in dailyAttendanceStudents)
            {
                // Get this student's track
                Track track = _trackRepo.GetTrackFor(student.iStudentID);

                if (track != null)
                {   
                    // For each calendar day for the whole school year
                    foreach (CalendarDay day in CalendarDay.GetCalendarDaysBetween(schoolYear.Starts, schoolYear.Ends, true))
                    {
                        // If this calendar day is instructional, use the daily blocks per day for the track as the value
                        // If this calendar day is not instructional, set it to zero
                        // We might not want to actually store zeroes in the database
                        if (track.Schedule.IsInstructional(day))
                        {
                            externalObjects.Add(new StudentExpectedAttendanceEntry()
                            {
                                iStudentID = student.iStudentID,
                                iSchoolYearID = schoolYear.ID,
                                Year = day.Year,
                                Month = day.Month,
                                Day = day.Day,
                                BlocksToday = track.DailyBlocksPerDay
                            });
                        }
                    }
                }
            }

            // Get student schedules for period attendance students
            SLStudentScheduleRepository _scheduleRepository = new SLStudentScheduleRepository(configFile.DatabaseConnectionString_SchoolLogic);
            Log("Loading student schedules...");
            Dictionary<int, StudentClassSchedule> _allStudentSchedules = _scheduleRepository.Get(periodAttendanceStudents.Select(x => x.iStudentID).ToList());
            Log("Finished loading student schedules.");

            foreach (Student student in periodAttendanceStudents)
            {                
                // Get this student's track
                Track track = _trackRepo.GetTrackFor(student.iStudentID);
                if (!_allStudentSchedules.ContainsKey(student.iStudentID)) { continue; }
                StudentClassSchedule schedule = _allStudentSchedules[student.iStudentID];

                // Make a StudentSchedule object to handle some of this automatically
                // I want to create a new StudentSchedule object (or a dictionary of them)
                // that can be easily queried for a specific calendar day

                if (track != null)
                {
                    // For each calendar day for the whole school year
                    foreach (CalendarDay day in CalendarDay.GetCalendarDaysBetween(schoolYear.Starts, schoolYear.Ends, true))
                    {
                        // If this calendar day is instructional, calculate the student's scheduled blocks
                        if (track.Schedule.IsInstructional(day))
                        {
                            // Calculate the student's schedule for today
                            int blocksToday = schedule.GetNumberOfScheduledBlocksOn(day);
                            if (blocksToday > 0)
                            {
                                externalObjects.Add(new StudentExpectedAttendanceEntry()
                                {
                                    iStudentID = student.iStudentID,
                                    iSchoolYearID = schoolYear.ID,
                                    Year = day.Year,
                                    Month = day.Month,
                                    Day = day.Day,
                                    BlocksToday = blocksToday
                                });
                            }  
                        }
                    }
                }
            }

            Log("Found " + externalObjects.Count() + " external objects");

            InternalStudentExpectedAttendanceRepository internalRepository = new InternalStudentExpectedAttendanceRepository(configFile.DatabaseConnectionString_Internal);
                       
            Log("Found " + internalRepository.TotalRecords(schoolYear.ID) + " internal objects");            

            /* ************************************************************ */
            // *
            // * This took over 6 hours to do, so we need to make a more efficient way of doing this.
            // * Perhaps the repository needs to store in a Dictionary<> mess instead of a single list
            // *
            /* ************************************************************ */
            // Compare for changes after here - all the above code was just loading stuff
            List<StudentExpectedAttendanceEntry> previouslyUnknown = new List<StudentExpectedAttendanceEntry>();
            List<StudentExpectedAttendanceEntry> needingUpdate = new List<StudentExpectedAttendanceEntry>();
            List<StudentExpectedAttendanceEntry> noLongerExistsInExternalSystem = new List<StudentExpectedAttendanceEntry>();

            int doneCount = 0;
            int totalExternalObjects = externalObjects.Count();
            decimal donePercent = 0;
            decimal doneThresholdPercent = (decimal)0.1;
            decimal doneThresholdIncrease = (decimal)0.1;

            foreach (StudentExpectedAttendanceEntry externalObject in externalObjects)
            {
                // Check to see if we know about this object already
                StudentExpectedAttendanceEntry internalObject = internalRepository.Get(externalObject.iStudentID, externalObject.iSchoolYearID, externalObject.Year, externalObject.Month, externalObject.Day);
                if (internalObject == null)
                {
                    previouslyUnknown.Add(externalObject);
                }

                // Check to see if this object requires an update
                if (internalObject != null)
                {
                    UpdateCheck check = internalObject.CheckIfUpdatesAreRequired(externalObject);
                    if ((check == UpdateCheck.UpdatesRequired) || (config.ForceUpdate))
                    {
                        needingUpdate.Add(externalObject);
                    }
                }

                doneCount++;
                donePercent = (decimal)((decimal)doneCount / (decimal)totalExternalObjects);
                if (donePercent > doneThresholdPercent)
                {
                    doneThresholdPercent = doneThresholdPercent + doneThresholdIncrease;
                    Log((int)(donePercent * 100) + "% finished inspecting objects");
                }

                if (doneCount == totalExternalObjects)
                {
                    Log("100% finished inspecting objects");
                }
            }

            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");

            // Commit these changes to the database
            if (previouslyUnknown.Count > 0)
            {
                if (config.AllowAdds)
                {
                    Log(" > Adding " + previouslyUnknown.Count() + " new objects");
                    internalRepository.Add(previouslyUnknown);
                }
                else
                {
                    Log(" > Not allowed to add, skipping " + previouslyUnknown.Count() + " adds");

                }
            }


            if (needingUpdate.Count > 0)
            {
                if (config.AllowUpdates)
                {
                    Log(" > Updating " + needingUpdate.Count() + " objects");
                    internalRepository.Update(needingUpdate);
                }
                else
                {
                    Log(" > Not allowed to do updates, skipping " + needingUpdate.Count() + " updates");
                }
            }

            Log("Finished syncing Expected Attendance");

        }
    }
}

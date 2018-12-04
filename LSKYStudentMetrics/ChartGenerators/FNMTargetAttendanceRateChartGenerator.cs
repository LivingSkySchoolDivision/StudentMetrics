using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Charts
{
    class FNMTargetAttendanceRateChartGenerator : HorizontalBarChartGenerator
    {
        List<string> _schoolGovIDBlacklist = new List<string>()
        {
            "2020500"
        };

        public FNMTargetAttendanceRateChartGenerator(string InternalConnectionString, FNMTargetAttendanceRateChart Options)
        {
            this.Title = "% Students with at least " + ((decimal)Options.TargetRate * 100).ToString("0") + "% Attendance Rate";
            this.SubTitle = Options.StartDate.ToShortDateString() + " to " + Options.EndDate.ToShortDateString();

            // Load all schools
            InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(InternalConnectionString);
            InternalStudentRepository _studentRepo = new InternalStudentRepository(InternalConnectionString);
            InternalStudentSchoolEnrolmentRepository _schoolStatusRepo = new InternalStudentSchoolEnrolmentRepository(InternalConnectionString);
            InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(InternalConnectionString, Options.StartDate, Options.EndDate);

            ChartData = new List<BarChartDataSeries>();

            // Set up the legend
            Legend = new Dictionary<int, string>()
            {
                {0,"Non First-Nations Students" },
                { 1,"First-Nations Students" }                
            };

            // Generate some data points
            foreach (School school in _schoolRepo.GetAll().Where(x => !_schoolGovIDBlacklist.Contains(x.GovernmentID)))
            {
                // Load school students
                List<Student> schoolStudents = _studentRepo.Get(_schoolStatusRepo.GetStudentIDsEnrolledOn(Options.StartDate, Options.EndDate, school.iSchoolID, true));

                // Skip schools that have no students
                if (schoolStudents.Count == 0)
                {
                    continue;
                }

                // Calculate each student's attendance rate for the given time period
                // Throw out rates that are -1, because they are invalid
                // Keep a running tally of all attendance rates, and of those from first nations students
                List<decimal> attendanceRatesNonFNM = new List<decimal>();
                List<decimal> attendanceRatesFNM = new List<decimal>();

                foreach (Student s in schoolStudents)
                {
                    StudentAttendanceRate sar = _attendanceRateRepo.GetForStudent(s.iStudentID, Options.StartDate, Options.EndDate);

                    decimal attendanceRate = sar.GetAttendanceRate(Options.StartDate, Options.EndDate);
                    if (attendanceRate != -1)
                    {                     
                        if (s.IsFirstNations)
                        {
                            attendanceRatesFNM.Add(attendanceRate);
                        } else
                        {
                            attendanceRatesNonFNM.Add(attendanceRate);
                        }
                    }
                }

                BarChartDataSeries schoolGraphData = new BarChartDataSeries()
                {
                    Label = school.ShortName
                };

                try
                {
                    decimal nonFNMAttendanceRate = (decimal)((decimal)attendanceRatesNonFNM.Count(x => x >= Options.TargetRate) / (decimal)attendanceRatesNonFNM.Count());
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = nonFNMAttendanceRate,
                        Label = (nonFNMAttendanceRate * 100).ToString("0.##") + "%",
                        ID = 0
                    });
                }
                catch
                {/*
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = 0,
                        Label = "Unknown",
                        ID = 0
                    });
                    */
                }

                try
                {
                    decimal fnmAttendanceRate = (decimal)((decimal)attendanceRatesFNM.Count(x => x >= Options.TargetRate) / (decimal)attendanceRatesFNM.Count());
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = fnmAttendanceRate,
                        Label = (fnmAttendanceRate * 100).ToString("0.##") + "%",
                        ID = 1
                    });
                }
                catch
                {/*
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = 0,
                        Label = "Unknown",
                        ID = 1
                    });*/
                }
                

                if (schoolGraphData.DataPoints.Count > 0)
                {
                    this.ChartData.Add(schoolGraphData);
                }
            }
        }


    }
}

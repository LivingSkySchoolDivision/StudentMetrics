using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Charts
{
    class TotalTargetAttendanceRateChartGenerator : HorizontalBarChartGenerator
    {
        List<string> _schoolGovIDBlacklist = new List<string>()
        {
            "2020500"
        };
               

        public TotalTargetAttendanceRateChartGenerator(string InternalConnectionString, ChartJob Options)
        {
            this.Title = "% Students with at least " + ((decimal)Options.TargetAttendanceRate * 100).ToString("0") + "% Attendance Rate";
            this.SubTitle = Options.StartDate.ToShortDateString() + " to " + Options.EndDate.ToShortDateString();

            // Load all schools
            InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(InternalConnectionString);
            InternalStudentRepository _studentRepo = new InternalStudentRepository(InternalConnectionString);
            InternalStudentSchoolEnrolmentRepository _schoolStatusRepo = new InternalStudentSchoolEnrolmentRepository(InternalConnectionString);
            InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(InternalConnectionString, Options.StartDate, Options.EndDate);
            InternalSchoolRepository schoolRepo = new InternalSchoolRepository(InternalConnectionString);

            // Determine limiting schools (if any)
            List<int> limitediSchoolIDs = new List<int>();
            if (Options.LimitSchools.Count > 0)
            {
                limitediSchoolIDs = Options.LimitSchools;
            }
            else
            {
                limitediSchoolIDs = schoolRepo.GetAllKnownSchoolIDs();
            }

            ChartData = new List<BarChartDataSeries>();
            
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
                List<decimal> attendanceRatesAllStudents = new List<decimal>();

                foreach (Student s in schoolStudents)
                {
                    StudentAttendanceRate sar = _attendanceRateRepo.GetForStudent(s.iStudentID, Options.StartDate, Options.EndDate);

                    decimal attendanceRate = sar.GetAttendanceRate(Options.StartDate, Options.EndDate);
                    if (attendanceRate != -1)
                    {
                        attendanceRatesAllStudents.Add(attendanceRate);                        
                    }
                }

                if (attendanceRatesAllStudents.Count == 0)
                {
                    continue;
                }

                BarChartDataSeries schoolGraphData = new BarChartDataSeries()
                {
                    Label = school.ShortName
                };

                if (attendanceRatesAllStudents.Count > 0)
                {
                    decimal totalAttendanceRate = (decimal)((decimal)attendanceRatesAllStudents.Count(x => x >= Options.TargetAttendanceRate) / (decimal)attendanceRatesAllStudents.Count());
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = totalAttendanceRate,
                        Label = (totalAttendanceRate * 100).ToString("0.##") + "%",
                        ID = 0
                    });
                }
                
                if (schoolGraphData.DataPoints.Count > 0)
                {
                    this.ChartData.Add(schoolGraphData);
                }
            }
        }

    }
}

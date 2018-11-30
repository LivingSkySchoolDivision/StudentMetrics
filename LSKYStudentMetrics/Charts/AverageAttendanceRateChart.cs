using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using LSSDMetricsLibrary.Extensions;

namespace LSSDMetricsLibrary.Charts
{
    public class AverageAttendanceRateChart : HorizontalBarChart
    {
        List<string> _schoolGovIDBlacklist = new List<string>()
        {
            "2020500"
        };
        public AverageAttendanceRateChart(string InternalConnectionString, DateTime startDate, DateTime endDate)
        {
            this.Title = "Average attendance rate";
            this.SubTitle = startDate.ToShortDateString() + " to " + endDate.ToShortDateString();

            // Load all schools
            InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(InternalConnectionString);
            InternalStudentRepository _studentRepo = new InternalStudentRepository(InternalConnectionString);
            InternalStudentSchoolEnrolmentRepository _schoolStatusRepo = new InternalStudentSchoolEnrolmentRepository(InternalConnectionString);
            InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(InternalConnectionString, startDate, endDate);

            ChartData = new List<BarChartDataSeries>();

            // Generate some data points
            foreach (School school in _schoolRepo.GetAll().Where(x => !_schoolGovIDBlacklist.Contains(x.GovernmentID)))
            {
                // Load school students
                List<Student> schoolStudents = _studentRepo.Get(_schoolStatusRepo.GetStudentIDsEnrolledOn(startDate, endDate, school.iSchoolID, true));

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
                    StudentAttendanceRate sar = _attendanceRateRepo.GetForStudent(s.iStudentID, startDate, endDate);

                    decimal attendanceRate = sar.GetAttendanceRate(startDate, endDate);
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
                    decimal averageAttendanceRate = attendanceRatesAllStudents.Average();
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = averageAttendanceRate,
                        Label = (averageAttendanceRate * 100).ToString("0.##") + "%",
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

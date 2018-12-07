using LSSDMetricsLibrary.Model.ChartParts;
using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Charts
{
    class ChartMonth
    {
        public string Label { get; set; }
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }
    }

    class SchoolYearTargetAttendanceChartGenerator : LineChartGenerator
    {
        private int ReturnLower(int one, int two)
        {
            if (one > two)
            {
                return two;
            } else
            {
                return one;
            }
        }

        public SchoolYearTargetAttendanceChartGenerator(string InternalConnectionString, ChartJob Options)
        {
            this.Title = "% Students with at least " + ((decimal)Options.TargetAttendanceRate * 100).ToString("0") + "% Attendance Rate";
            this.SubTitle = Options.StartDate.ToShortDateString() + " to " + Options.EndDate.ToShortDateString();
            this.ShowValuesInChart = true;

            // Labels / Data points will be one for each month
       

            this.Lines = new List<LineChartLine>();

            // DATA NEEDED FOR THIS REPORT
            // - Load all enrolled students
            // - For each month within the specified date range, calculate every student's attendance rate
            // - Find the final values for each month
            // This chart is done by the School Year, not arbitrary dates

            InternalStudentRepository studentRepo = new InternalStudentRepository(InternalConnectionString);
            InternalStudentSchoolEnrolmentRepository schoolEnrolmentRepo = new InternalStudentSchoolEnrolmentRepository(InternalConnectionString);
            InternalStudentAttendanceRateRepository attendanceRateRepo = new InternalStudentAttendanceRateRepository(InternalConnectionString, Options.StartDate, Options.EndDate);

            LineChartLine fnmStudentsLine = new LineChartLine() { Label = "FNM Students" };
            LineChartLine nonFNMStudentsLine = new LineChartLine() { Label = "Non-FNM Students" };

            // Find the school year that the specified dates fit into
            // If the dates span into two or more school years, use the first one detected
            // The chart will always start at the beginning of the school year and end at the end of the school year

            int schoolYear = ReturnLower(Parsers.FindSchoolYear(Options.StartDate), Parsers.FindSchoolYear(Options.EndDate));

            List<ChartMonth> schoolYears = new List<ChartMonth>()
            {
                new ChartMonth{ Label="AUG", Starts=new DateTime(schoolYear, 8, 1), Ends=new DateTime(schoolYear, 8, DateTime.DaysInMonth(schoolYear, 8)) },
                new ChartMonth{ Label="SEP", Starts=new DateTime(schoolYear, 9, 1), Ends=new DateTime(schoolYear, 9, DateTime.DaysInMonth(schoolYear, 9)) },
                new ChartMonth{ Label="OCT", Starts=new DateTime(schoolYear, 10, 1), Ends=new DateTime(schoolYear, 10, DateTime.DaysInMonth(schoolYear, 10)) },
                new ChartMonth{ Label="NOV", Starts=new DateTime(schoolYear, 11, 1), Ends=new DateTime(schoolYear, 11, DateTime.DaysInMonth(schoolYear, 11)) },
                new ChartMonth{ Label="DEC", Starts=new DateTime(schoolYear, 12, 1), Ends=new DateTime(schoolYear, 12, DateTime.DaysInMonth(schoolYear, 12)) },
                new ChartMonth{ Label="JAN", Starts=new DateTime(schoolYear + 1, 1, 1), Ends=new DateTime(schoolYear + 1, 1, DateTime.DaysInMonth(schoolYear + 1, 1)) },
                new ChartMonth{ Label="FEB", Starts=new DateTime(schoolYear + 1, 2, 1), Ends=new DateTime(schoolYear + 1, 2, DateTime.DaysInMonth(schoolYear + 1, 2)) },
                new ChartMonth{ Label="MAR", Starts=new DateTime(schoolYear + 1, 3, 1), Ends=new DateTime(schoolYear + 1, 3, DateTime.DaysInMonth(schoolYear + 1, 3)) },
                new ChartMonth{ Label="APR", Starts=new DateTime(schoolYear + 1, 4, 1), Ends=new DateTime(schoolYear + 1, 4, DateTime.DaysInMonth(schoolYear + 1, 4)) },
                new ChartMonth{ Label="MAY", Starts=new DateTime(schoolYear + 1, 5, 1), Ends=new DateTime(schoolYear + 1, 5, DateTime.DaysInMonth(schoolYear + 1, 5)) },
                new ChartMonth{ Label="JUN", Starts=new DateTime(schoolYear + 1, 6, 1), Ends=new DateTime(schoolYear + 1, 6, DateTime.DaysInMonth(schoolYear + 1, 6)) },
                new ChartMonth{ Label="JUL", Starts=new DateTime(schoolYear + 1, 7, 1), Ends=new DateTime(schoolYear + 1, 7, DateTime.DaysInMonth(schoolYear + 1, 7)) },
            };

            this.Labels = schoolYears.Select(x => x.Label).ToList();

            foreach (ChartMonth month in schoolYears)
            {
                // Check to see if this month falls within the specified time period
                // If it doesn't, skip it
                if (month.Starts > Options.EndDate) { continue; }
                if (month.Ends < Options.StartDate) { continue; }

                // Clamp the start and end dates to the start and end dates specified
                if (month.Starts < Options.StartDate) { month.Starts = Options.StartDate; }
                if (month.Ends > Options.EndDate) { month.Ends = Options.EndDate; }
                               
                // If the month is the current month, cap the end date at today, because future data won't exist yet
                if ((month.Starts.Month == DateTime.Today.Month) && (month.Starts.Year == DateTime.Today.Year))
                {
                    if (month.Ends > DateTime.Today)
                    {
                        month.Ends = DateTime.Today;
                    }
                }

                decimal dataPointThisMonth_NonFNM = (decimal)-1.0;
                decimal dataPointThisMonth_FNM = (decimal)-1.0;

                // Now, load all the enrolled students during the start and end of the month
                // and calculate their average attendances

                List<Student> enrolledStudentsThisMonth = studentRepo.Get(schoolEnrolmentRepo.GetStudentIDsEnrolledOn(month.Starts, month.Ends, true));
                if (enrolledStudentsThisMonth.Count == 0)
                {
                    continue;
                }

                List<decimal> attendanceRatesNonFNM = new List<decimal>();
                List<decimal> attendanceRatesFNM = new List<decimal>();

                foreach(Student s in enrolledStudentsThisMonth)
                {
                    StudentAttendanceRate sar = attendanceRateRepo.GetForStudent(s.iStudentID, month.Starts, month.Ends);

                    decimal attendanceRate = sar.GetAttendanceRate(month.Starts, month.Ends);
                    if (attendanceRate != -1)
                    {
                        if (s.IsFirstNations)
                        {
                            attendanceRatesFNM.Add(attendanceRate);
                        }
                        else
                        {
                            attendanceRatesNonFNM.Add(attendanceRate);
                        }
                    }
                }

                // Now, find the number of the rates that are greater than or equal to the target
                try
                {
                    dataPointThisMonth_NonFNM = (decimal)((decimal)attendanceRatesNonFNM.Count(x => x > Options.TargetAttendanceRate) / (decimal)attendanceRatesNonFNM.Count());
                }
                catch { }

                try
                {
                    dataPointThisMonth_FNM = (decimal)((decimal)attendanceRatesFNM.Count(x => x > Options.TargetAttendanceRate) / (decimal)attendanceRatesFNM.Count());
                }
                catch { }
                
                // Add the data point to the line
                if (dataPointThisMonth_NonFNM > -1)
                {
                    nonFNMStudentsLine.LineDataPoints.Add(month.Label, dataPointThisMonth_NonFNM);
                }

                if (dataPointThisMonth_FNM > -1)
                {
                    fnmStudentsLine.LineDataPoints.Add(month.Label, dataPointThisMonth_FNM);
                }
            }

            // Finally, add the lines to the chart
            this.Lines.Add(nonFNMStudentsLine);
            this.Lines.Add(fnmStudentsLine);
        }

    }
}

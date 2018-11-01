using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class StudentExpectedAttendance
    {
        public int iStudentID { get; set; }

        // Dictionary<year, Dictionary<month, Dictionary<day, blocks>>>
        private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _attendanceArray { get; set; }

        public int ExpectedBlocksOn(DateTime thisDate)
        {
            return ExpectedBlocksOn(thisDate.Year, thisDate.Month, thisDate.Day);
        }

        public int ExpectedBlocksOn(int year, int month, int day)
        {
            if (_attendanceArray.ContainsKey(year))
            {
                if (_attendanceArray[year].ContainsKey(month))
                {
                    if (_attendanceArray[year][month].ContainsKey(day))
                    {
                        return _attendanceArray[year][month][day];
                    }
                }
            }
            return 0;
        }

        public void Add(StudentExpectedAttendance additionalAttendance)
        {
            foreach (int year in additionalAttendance._attendanceArray.Keys)
            {
                if (!this._attendanceArray.ContainsKey(year))
                {
                    this._attendanceArray.Add(year, new Dictionary<int, Dictionary<int, int>>());
                }

                foreach (int month in additionalAttendance._attendanceArray[year].Keys)
                {
                    if (!this._attendanceArray[year].ContainsKey(month))
                    {
                        this._attendanceArray[year].Add(month, new Dictionary<int, int>());
                    }

                    foreach (int day in additionalAttendance._attendanceArray[year][month].Keys)
                    {
                        if (!this._attendanceArray[year][month].ContainsKey(day))
                        {
                            this._attendanceArray[year][month].Add(day, 0);
                        }

                        this._attendanceArray[year][month][day] = additionalAttendance._attendanceArray[year][month][day];
                    }
                }
            }
            
            // The additionalAttendance object can now be discarded
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentExpectedAttendance
    {
        public int iStudentID { get; set; }

        // Dictionary<year, Dictionary<month, Dictionary<day, blocks>>>
        private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _attendanceArray { get; set; }

        public StudentExpectedAttendance()
        {
            this._attendanceArray = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
        }

        public StudentExpectedAttendance(List<StudentExpectedAttendanceEntry> expectedAttendanceEntries)
        {
            int iStudentID = 0;
            this._attendanceArray = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            foreach (StudentExpectedAttendanceEntry entry in expectedAttendanceEntries)
            {
                if (iStudentID != entry.iStudentID)
                {
                    iStudentID = entry.iStudentID;
                }

                this.Add(entry);
            }
            this.iStudentID = iStudentID;
        }              

        public StudentExpectedAttendance(int iStudentID, List<StudentExpectedAttendanceEntry> expectedAttendanceEntries)
        {
            this.iStudentID = iStudentID;
            this._attendanceArray = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            foreach(StudentExpectedAttendanceEntry entry in expectedAttendanceEntries)
            {
                this.Add(entry);
            }
        }

        public int ExpectedBlocksOn(CalendarDay thisDay)
        {
            return ExpectedBlocksOn(thisDay.Year, thisDay.Month, thisDay.Day);
        }

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
        
        public void Add(StudentExpectedAttendanceEntry entry)
        {
            if (entry.iStudentID == this.iStudentID)
            {
                if (entry.BlocksToday > 0)
                {
                    if (!this._attendanceArray.ContainsKey(entry.Year))
                    {
                        this._attendanceArray.Add(entry.Year, new Dictionary<int, Dictionary<int, int>>());
                    }

                    if (!this._attendanceArray[entry.Year].ContainsKey(entry.Month))
                    {
                        this._attendanceArray[entry.Year].Add(entry.Month, new Dictionary<int, int>());
                    }

                    if (!this._attendanceArray[entry.Year][entry.Month].ContainsKey(entry.Day))
                    {
                        this._attendanceArray[entry.Year][entry.Month].Add(entry.Day, 0);
                    }

                    if (this._attendanceArray[entry.Year][entry.Month][entry.Day] < entry.BlocksToday)
                    {
                        this._attendanceArray[entry.Year][entry.Month][entry.Day] += entry.BlocksToday;
                    }
                }
            } else
            {
                throw new Exception("Cannot add expected attendance from another student");
            }
        }

        public void Add(List<StudentExpectedAttendanceEntry> entries)
        {
            foreach(StudentExpectedAttendanceEntry entry in entries)
            {
                this.Add(entry);
            }
        }

    }
}

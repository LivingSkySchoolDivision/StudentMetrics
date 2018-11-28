using LSSDMetricsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentAttendanceRate
    {
        public StudentAttendanceRate(int iStudentID)
        {
            this.iStudentID = iStudentID;
        }
        
        private int iStudentID { get; set; }
        public StudentExpectedAttendance ExpectedAttendance { get; set; }
        public StudentAbsenceHelper Absences { get; set; }

        public int GetExpectedAttendance(DateTime thisDay)
        {
            return ExpectedAttendance.ExpectedBlocksOn(thisDay);
        }

        public int GetNumAbsences(DateTime thisDay)
        {
            return Absences.NegativeAttendanceOn(thisDay);
        }

        public decimal GetAttendanceRate(DateTime thisDay)
        {
            int expectedBlocks = GetExpectedAttendance(thisDay);
            int absencesToday = GetNumAbsences(thisDay);

            return (decimal)(((decimal)expectedBlocks - (decimal)absencesToday) / (decimal)expectedBlocks);
        }
               
        public decimal GetAttendanceRate(DateTime from, DateTime to)
        {
            int expectedBlocks = GetExpectedAttendance(from, to);
            int absencesToday = GetNumAbsences(from, to);

            return (decimal)(((decimal)expectedBlocks - (decimal)absencesToday) / (decimal)expectedBlocks);
        }

        public int GetExpectedAttendance(DateTime from, DateTime to)
        {
            int returnMe = 0;

            foreach(DateTime day in Parsers.GetEachDayBetween(from, to))
            {
                returnMe += GetExpectedAttendance(day);
            }

            return returnMe;
        }

        public int GetNumAbsences(DateTime from, DateTime to)
        {
            int returnMe = 0;

            foreach (DateTime day in Parsers.GetEachDayBetween(from, to))
            {
                returnMe += GetNumAbsences(day);
            }

            return returnMe;

        }


    }
}

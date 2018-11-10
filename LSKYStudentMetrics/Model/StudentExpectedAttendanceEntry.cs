using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class StudentExpectedAttendanceEntry
    {
        public int iStudentID { get; set; }
        public int iSchoolYearID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int BlocksToday { get; set; }

        public UpdateCheck CheckIfUpdatesAreRequired(StudentExpectedAttendanceEntry obj)
        { 
            // For this object, the "ID" is every field except blockstoday
            if (
                (this.iStudentID != obj.iStudentID) ||
                (this.Year != obj.Year) ||
                (this.Month != obj.Month) ||
                (this.Day != obj.Day)
                )
            {
                return UpdateCheck.NotSameObject;
            }
            
            // Check all properties of the objects to see if they are different
            int updates = 0;

            if (!this.BlocksToday.Equals(obj.BlocksToday)) { updates++; }

            if (updates == 0)
            {
                return UpdateCheck.NoUpdateRequired;
            }
            else
            {
                return UpdateCheck.UpdatesRequired;
            }

        }
    }
}

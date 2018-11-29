using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentExpectedAttendanceEntry
    {
        public int iStudentID { get; set; }
        public int iSchoolYearID { get; set; }
        public DateTime Date { get; set; }        
        public int BlocksToday { get; set; }

        public UpdateCheck CheckIfUpdatesAreRequired(StudentExpectedAttendanceEntry obj)
        { 
            // For this object, the "ID" is every field except blockstoday
            if (
                (this.iStudentID != obj.iStudentID) ||
                (this.Date.Year != obj.Date.Year) ||
                (this.Date.Month != obj.Date.Month) ||
                (this.Date.Date != obj.Date.Date)
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

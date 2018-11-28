using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentGradePlacement
    {
        public int iStudentID { get; set; }
        public int iGradeID { get; set; }
        public int iSchoolYearID { get; set; }
        public GradeLevel GradeLevel { get; set; }
        public SchoolYear SchoolYear { get; set; }

        public override string ToString()
        {
            return "{ StudentGradePlacement iStudentID:" + this.iStudentID + ", iSchoolYearID:" + this.iSchoolYearID + ", iGradeID:" + this.iGradeID + "}";
        }

        public UpdateCheck CheckIfUpdatesAreRequired(StudentGradePlacement obj)
        {
            // Check to make sure that the ID matches, and return -1 if it does not
            if (this.iSchoolYearID != obj.iSchoolYearID)
            {
                if (this.iStudentID != obj.iStudentID)
                {
                    return UpdateCheck.NotSameObject;
                }
            }

            // Check all properties of the objects to see if they are different
            int updates = 0;

            if (!this.iGradeID.Equals(obj.iGradeID)) { updates++; }

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

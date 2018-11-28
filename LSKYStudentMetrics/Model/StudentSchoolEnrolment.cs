using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentSchoolEnrolment
    {
        public int ID { get; set; }
        public int iStudentID { get; set; }
        public int iSchoolID { get; set; }
        public DateTime InDate { get; set; }
        public DateTime OutDate { get; set; }

        public string InStatus { get; set; }
        public string OutStatus { get; set; }
        public int OutsideTrackID { get; set; }

        public UpdateCheck CheckIfUpdatesAreRequired(StudentSchoolEnrolment obj)
        {
            // Check to make sure that the ID matches, and return -1 if it does not
            if (this.ID != obj.ID)
            {
                return UpdateCheck.NotSameObject;
            }

            // Check all properties of the objects to see if they are different
            int updates = 0;

            if (!this.iStudentID.Equals(obj.iStudentID)) { updates++; }
            if (!this.iSchoolID.Equals(obj.iSchoolID)) { updates++; }
            if (!this.InDate.Equals(obj.InDate)) { updates++; }
            if (!this.OutDate.Equals(obj.OutDate)) { updates++; }
            if (!this.InStatus.Equals(obj.InStatus)) { updates++; }
            if (!this.OutStatus.Equals(obj.OutStatus)) { updates++; }
            if (!this.OutsideTrackID.Equals(obj.OutsideTrackID)) { updates++; }

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

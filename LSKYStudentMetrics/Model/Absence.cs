using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class Absence
    {
        public int ID { get; set; }
        public AbsenceStatus Status { get; set; }
        public AbsenceReason Reason { get; set; }
        public DateTime Date { get; set; }
        public int iSchoolID { get; set; }
        public int iStudentID { get; set; }
        public int BlockNumber { get; set; }
        public decimal Minutes { get; set; }
        public int iClassID { get; set; }
        public int iHomeRoomID { get; set; }
        public int iReasonID { get; set; }
        public int iStatusID { get; set; }
        public int iStaffID { get; set; }
        public int iSchoolYearID { get; set; }

        public bool CountsAgainstAttendanceRate {
            get
            {
                if (this.Status?.Content.ToLower() == "absent")
                {
                    if (this.Reason == null)
                    {
                        return true;
                    }

                    if (this.Reason.Content == string.Empty)
                    {
                        return true;
                    }

                    if (this.Reason?.Content.ToLower() == "known reason")
                    {
                        return true;
                    }

                    if (this.Reason?.Content.ToLower() == "medical")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override string ToString()
        {
            return "{ABSENCE DATE:" + this.Date.ToShortDateString() + " STATUS:" + this.Status + " REASON:" + this.Reason + " COUNTS:" + this.CountsAgainstAttendanceRate + "}";
        }

        public UpdateCheck CheckIfUpdatesAreRequired(Absence obj)
        {
            // Check to make sure that the ID matches, and return -1 if it does not
            if (this.ID != obj.ID)
            {
                return UpdateCheck.NotSameObject;
            }

            // Check all properties of the objects to see if they are different
            int updates = 0;

            if (!this.Date.Equals(obj.Date)) { updates++; }
            if (!this.iSchoolID.Equals(obj.iSchoolID)) { updates++; }
            if (!this.iStudentID.Equals(obj.iStudentID)) { updates++; }
            if (!this.BlockNumber.Equals(obj.BlockNumber)) { updates++; }
            if (!this.Minutes.Equals(obj.Minutes)) { updates++; }
            if (!this.iClassID.Equals(obj.iClassID)) { updates++; }
            if (!this.iHomeRoomID.Equals(obj.iHomeRoomID)) { updates++; }
            if (!this.iReasonID.Equals(obj.iReasonID)) { updates++; }
            if (!this.iStatusID.Equals(obj.iStatusID)) { updates++; }
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

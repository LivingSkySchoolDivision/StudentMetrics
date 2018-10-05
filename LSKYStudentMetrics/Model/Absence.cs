using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    class Absence
    {
        public int iAttendanceID { get; set; }
        public AbsenceStatus Status { get; set; }
        public AbsenceReason Reason { get; set; }

        public DateTime Date { get; set; }
        public int iSchoolID { get; set; }
        public int iStudentID { get; set; }

        public bool IsAbsence { get; set; }
        public bool IsPresence { get { return !this.IsAbsence; } }

        public bool IsDaily { get; set; }
        public int BlockNumber { get; set; }
        public decimal LateMinutes { get; set; }
        public int iClassID { get; set; }
        public int iReasonID { get; set; }
        public int iStatusID { get; set; }

    }
}

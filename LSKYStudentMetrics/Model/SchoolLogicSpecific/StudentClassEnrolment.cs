using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentClassEnrolment
    {
        public int iStudentID { get; set; }
        public int iClassID { get; set; }

        public SchoolClass Class { get; set; }
        public DateTime InDate { get; set; }
        public DateTime OutDate { get; set; }
        public EnrolmentType Status { get; set; }

        public override string ToString()
        {
            return "{ StudentClassEnrolment student:"+this.iStudentID+", class:"+this.iClassID+", status:"+this.Status+", indate:"+this.InDate.ToShortDateString()+", outdate:"+this.OutDate.ToShortDateString()+" }";
        }
    }
}

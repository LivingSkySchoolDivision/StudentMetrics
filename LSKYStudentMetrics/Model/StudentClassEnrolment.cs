using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    class StudentClassEnrolment
    {
        public SchoolClass Class { get; set; }
        public DateTime InDate { get; set; }
        public DateTime OutDate { get; set; }
        public EnrolmentType Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    class Student
    {
        public int iStudentID { get; set; }
        public string cStudentNumber { get; set; }

        public bool IsFirstNations { get; set; }

        public char Gender { get; set; }

        public List<StudentGradePlacement> GradePlacements { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class Term
    {        
        public int ID { get; set; }
        public int iSchoolID { get; set; }
        public string Name { get; set; }

        public Track Track { get; set; }
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }
    }
}

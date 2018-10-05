using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class SchoolYear
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }

        public override string ToString()
        {
            return "{ SchoolYear ID:" + this.ID + " Name:" + this.Name + " Starts:" + this.Starts + " Ends:" + this.Ends + " }";
        }
    }
}

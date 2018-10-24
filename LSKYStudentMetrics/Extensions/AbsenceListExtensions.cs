using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Extensions
{
    public static class AbsenceListExtensions
    {

        public static Absence GetWithID(this List<Absence> list, int id)
        {
            return list.Where(x => x.ID == id).FirstOrDefault();
        }
            

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Model
{
    class StudentSchedule
    {
        List<SchoolClass> allEnrolledClasses = new List<SchoolClass>();
               
        public int GetScheduledClassesOn(DateTime date)
        {
            int returnMe = 0;

            // Get the schedule for all enrolled classes on the given day
            foreach(SchoolClass sc in allEnrolledClasses)
            {
                returnMe += sc.Schedule.GetBlocksScheduledOn(date).Count();
            }

            return returnMe;
        }

    }
}

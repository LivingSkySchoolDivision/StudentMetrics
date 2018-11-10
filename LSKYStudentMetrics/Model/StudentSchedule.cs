using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class StudentClassSchedule
    {
        public int iStudentID { get; set; }

        public List<StudentClassEnrolment> AllEnrolledClasses = new List<StudentClassEnrolment>();
               
        public int GetNumberOfScheduledBlocksOn(DateTime date)
        {
            int returnMe = 0;

            // Get the schedule for all enrolled classes on the given day
            foreach(StudentClassEnrolment sec in AllEnrolledClasses)
            {
                returnMe += sec.Class.Schedule.GetBlocksScheduledOn(date).Count();
            }

            return returnMe;
        }

        // The GetNumberOfScheduledBlocksOn is the better method here, because some classes could be sheduled multiple times
        // For classes spanning multiple blocks, attendance is technically taken once per block
        private List<StudentClassEnrolment> GetScheduledClassesOn(DateTime date)
        {
            List<StudentClassEnrolment> returnMe = new List<StudentClassEnrolment>();
            
            foreach(StudentClassEnrolment sec in AllEnrolledClasses)
            {
                if ((sec.InDate <= date) && ((sec.OutDate <= new DateTime(1901,1,1)) || (sec.OutDate >= date)))
                {
                    returnMe.Add(sec);
                }
            }

            return returnMe;
        }



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentClassSchedule
    {
        public int iStudentID { get; set; }

        public List<StudentClassEnrolment> AllEnrolledClasses = new List<StudentClassEnrolment>();
               
        public int GetNumberOfScheduledBlocksOn(DateTime date)
        {
            int returnMe = 0;

            // Get the schedule for all enrolled classes on the given day
            // Make sure the student is actually enrolled in the class at this time
            foreach(StudentClassEnrolment sec in AllEnrolledClasses.Where(x => (x.InDate <= date) && ((x.OutDate >= date) || (x.OutDate < new DateTime(1901,1,1)))))
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

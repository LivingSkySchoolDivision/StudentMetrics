using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    public class SLStudentScheduleRepository
    {
        private SLStudentClassEnrolmentRepository _enrolmentRepo;
        private string SQLConnectionString = string.Empty;


        public SLStudentScheduleRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            this._enrolmentRepo = new SLStudentClassEnrolmentRepository(SQLConnectionString);
        }


        public StudentClassSchedule Get(int iStudentID)
        {
            return new StudentClassSchedule()
            {
                iStudentID = iStudentID,
                AllEnrolledClasses = _enrolmentRepo.GetForStudent(iStudentID)
            };
        }


        public Dictionary<int, StudentClassSchedule> Get(List<int> iStudentID)
        {
            Dictionary<int, StudentClassSchedule> returnMe = new Dictionary<int, StudentClassSchedule>();
            Dictionary<int, List<StudentClassEnrolment>> _studentEnrolledClasses = _enrolmentRepo.GetForStudents(iStudentID);
            
            foreach(int id in iStudentID)
            {
                StudentClassSchedule schedule = new StudentClassSchedule()
                {
                    iStudentID = id,
                    AllEnrolledClasses = _studentEnrolledClasses.ContainsKey(id) ? _studentEnrolledClasses[id] : new List<StudentClassEnrolment>()
                };
                returnMe.Add(id, schedule);
            }

            return returnMe;
        }
    }
}

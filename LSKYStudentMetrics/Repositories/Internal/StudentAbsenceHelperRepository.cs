using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.Internal
{
    class StudentAbsenceHelperRepository
    {
        InternalAbsenceRepository _absenceRepo;

        public StudentAbsenceHelperRepository(string SQLConnectionString)
        {
            _absenceRepo = new InternalAbsenceRepository(SQLConnectionString);
        }

        public StudentAbsenceHelperRepository(string SQLConnectionString, DateTime startDate, DateTime endDate)
        {
            _absenceRepo = new InternalAbsenceRepository(SQLConnectionString, startDate, endDate);
        }

        public StudentAbsenceHelper GetForStudent(int iStudentID, DateTime startDate, DateTime endDate)
        {
            StudentAbsenceHelper returnMe = new StudentAbsenceHelper(iStudentID, _absenceRepo.GetForStudent(iStudentID, startDate, endDate));
            return returnMe;
        }
    }
}

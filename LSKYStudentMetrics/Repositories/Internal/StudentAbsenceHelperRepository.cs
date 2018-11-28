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

        public StudentAbsenceHelper GetForStudent(int iStudentID)
        {
            StudentAbsenceHelper returnMe = new StudentAbsenceHelper(iStudentID, _absenceRepo.GetForStudent(iStudentID));
            return returnMe;
        }
    }
}

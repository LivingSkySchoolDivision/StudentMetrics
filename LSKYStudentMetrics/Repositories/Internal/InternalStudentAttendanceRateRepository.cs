using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.Internal
{
    public class InternalStudentAttendanceRateRepository
    {
        private InternalStudentExpectedAttendanceRepository _expectedAttendanceRepo;
        private StudentAbsenceHelperRepository _studentAbsenceHelperRepo;
        
        public InternalStudentAttendanceRateRepository(string SQLConnectionString)
        {
            this._expectedAttendanceRepo = new InternalStudentExpectedAttendanceRepository(SQLConnectionString);
            this._studentAbsenceHelperRepo = new StudentAbsenceHelperRepository(SQLConnectionString);
        }

        public StudentAttendanceRate GetForStudent(int iStudentID)
        {
            return new StudentAttendanceRate(iStudentID)
            {
                ExpectedAttendance = _expectedAttendanceRepo.GetForStudent(iStudentID),
                Absences = _studentAbsenceHelperRepo.GetForStudent(iStudentID)
            };
        }
    }
}

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

        public StudentAttendanceRate GetForStudent(int iStudentID, DateTime startDate, DateTime endDate)
        {
            return new StudentAttendanceRate(iStudentID)
            {
                ExpectedAttendance = _expectedAttendanceRepo.GetForStudent(iStudentID, startDate, endDate),
                Absences = _studentAbsenceHelperRepo.GetForStudent(iStudentID, startDate, endDate)
            };
        }
    }
}

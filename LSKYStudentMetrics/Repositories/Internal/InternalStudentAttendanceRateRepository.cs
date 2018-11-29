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

        /// <summary>
        /// Speed up repeated processes by preloading data for the specified dates
        /// </summary>
        /// <param name="SQLConnectionString"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public InternalStudentAttendanceRateRepository(string SQLConnectionString, DateTime startDate, DateTime endDate)
        {
            this._expectedAttendanceRepo = new InternalStudentExpectedAttendanceRepository(SQLConnectionString, startDate, endDate);
            this._studentAbsenceHelperRepo = new StudentAbsenceHelperRepository(SQLConnectionString, startDate, endDate);
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

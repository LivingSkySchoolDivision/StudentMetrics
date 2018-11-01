using LSKYStudentMetrics;
using LSKYStudentMetrics.Repositories.SchoolLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.SyncEngine
{
    class StudentExpectedBlocksSync
    {
        public static void Sync(ConfigFile configFile, LogDelegate Log)
        {
            Log("========= EXPECTED ATTENDANCE ========= ");

            // Load all students that have enrolled classes
            //  - a DISTINCT() on the enrollment table should do nicely here

            // Load all of those student's schedules
            // - Load each class's schedule, then combine into the student's own schedule

            SLStudentRepository _studentRepo = new SLStudentRepository(configFile.DatabaseConnectionString_SchoolLogic);

            Log("Active students: " + _studentRepo.GetActive().Count());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    class SLClassRepository
    {
        private SLClassScheduleRepository _classScheduleRepo;
        private string SQLConnectionString = string.Empty;


        public SLClassRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _classScheduleRepo = new SLClassScheduleRepository(SQLConnectionString);
        }

    }
}

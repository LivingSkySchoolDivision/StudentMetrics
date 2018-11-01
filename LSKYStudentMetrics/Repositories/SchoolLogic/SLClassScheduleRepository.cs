using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    class SLClassScheduleRepository
    {
        private SLTrackRepository _trackRepository;
        private string SQLConnectionString = string.Empty;


        public SLClassScheduleRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _trackRepository = new SLTrackRepository(SQLConnectionString);

        }


    }
}

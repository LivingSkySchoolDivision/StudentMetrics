using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    class SLStudentClassEnrolmentRepository
    {
        SLClassRepository _classRepo;
        private string SQLConnectionString = string.Empty;

        public SLStudentClassEnrolmentRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _classRepo = new SLClassRepository(SQLConnectionString);
        }


    }
}

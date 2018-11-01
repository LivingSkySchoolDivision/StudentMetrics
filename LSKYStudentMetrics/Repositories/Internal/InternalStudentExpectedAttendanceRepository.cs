using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.Internal
{
    public class InternalStudentExpectedAttendanceRepository
    {
        private const string _selectSQL = "SELECT * FROM StudentExpectedBlocksPerDay";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, StudentExpectedAttendance> _cache = new Dictionary<int, StudentExpectedAttendance>();


        public InternalStudentExpectedAttendanceRepository()
        {

        }

        private StudentExpectedAttendance dataReaderToStudentExpectedAttendance(SqlDataReader dataReader)
        {
            return null;
        }



    }
}

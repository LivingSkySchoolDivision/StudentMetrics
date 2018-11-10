using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    class SLClassRepository
    {
        private SLClassScheduleRepository _classScheduleRepo;
        private string SQLConnectionString = string.Empty;
        private const string selectSQL = "SELECT DISTINCT(iClassID) from Class";

        private Dictionary<int, SchoolClass> _cache;
        
        private SchoolClass dataReaderToObject(SqlDataReader dataReader)
        {
            int classID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim());

            return new SchoolClass()
            {
                ID = classID,
                Schedule = _classScheduleRepo.GetForClass(classID)
            };
        }

        public SLClassRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _classScheduleRepo = new SLClassScheduleRepository(SQLConnectionString);
            this._cache = new Dictionary<int, SchoolClass>();

            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = selectSQL;
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            SchoolClass c = dataReaderToObject(dataReader);
                            if (c != null)
                            {
                                _cache.Add(c.ID, c);
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

        }

        public SchoolClass Get(int iClassID)
        {
            if (_cache.ContainsKey(iClassID))
            {
                return _cache[iClassID];
            } else
            {
                return null;
            }
        }

    }
}

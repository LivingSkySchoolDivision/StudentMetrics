using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    public class SLAbsenceStatusRepository
    {
        private const string SelectSQL = "SELECT iAttendanceStatusID, cName FROM AttendanceStatus";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, AbsenceStatus> _cache = new Dictionary<int, AbsenceStatus>();

        private AbsenceStatus dataReaderToObject(SqlDataReader dataReader)
        {
            return new AbsenceStatus()
            {
                ID = Parsers.ParseInt(dataReader["iAttendanceStatusID"].ToString().Trim()),
                Content = dataReader["cName"].ToString().Trim()
            };
        }

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, AbsenceStatus>();
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = SelectSQL;
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                AbsenceStatus parsedObject = dataReaderToObject(dataReader);
                                if (parsedObject != null)
                                {
                                    _cache.Add(parsedObject.ID, parsedObject);
                                }
                            }
                        }
                        sqlCommand.Connection.Close();
                    }
                }
            }
            else
            {
                throw new InvalidConnectionStringException("Connection string is empty");
            }
        }

        public SLAbsenceStatusRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _refreshCache();
        }

        public List<int> GetAllIDs()
        {
            return _cache.Keys.ToList();
        }

        public AbsenceStatus Get(int iAbsenceStatusID)
        {
            if (_cache.ContainsKey(iAbsenceStatusID))
            {
                return _cache[iAbsenceStatusID];
            }
            else
            {
                return null;
            }
        }

        public List<AbsenceStatus> GetAll()
        {
            return _cache.Values.ToList();
        }
    }
}

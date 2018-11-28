using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.Internal
{
    public class InternalAbsenceStatusRepository
    {
        private const string SelectSQL = "SELECT iAttendanceStatusID, cStatus FROM AttendanceStatuses";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, AbsenceStatus> _cache = new Dictionary<int, AbsenceStatus>();

        private AbsenceStatus dataReaderToObject(SqlDataReader dataReader)
        {
            return new AbsenceStatus()
            {
                ID = Parsers.ParseInt(dataReader["iAttendanceStatusID"].ToString().Trim()),
                Content = dataReader["cStatus"].ToString().Trim()
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

        public InternalAbsenceStatusRepository(string SQLConnectionString)
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

        public void Add(List<AbsenceStatus> objs)
        {
            // Add to database
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.Connection.Open();
                        foreach (AbsenceStatus obj in objs)
                        {
                            sqlCommand.CommandText = "INSERT INTO AttendanceStatuses(iAttendanceStatusID, cStatus) VALUES(@STATUSID,@CSTATUS)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STATUSID", obj.ID);
                            sqlCommand.Parameters.AddWithValue("CSTATUS", obj.Content);
                            sqlCommand.ExecuteNonQuery();
                        }
                        sqlCommand.Connection.Close();
                    }
                }
            }
            else
            {
                throw new InvalidConnectionStringException("Connection string is empty");
            }

            // Refresh cache from database
            _refreshCache();
        }

        public void Update(List<AbsenceStatus> objs)
        {
            // Update database
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.Connection.Open();
                        foreach (AbsenceStatus obj in objs)
                        {
                            sqlCommand.CommandText = "UPDATE AttendanceStatuses SET cStatus=@CSTATUS WHERE iAttendanceStatusID=@STATUSID";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STATUSID", obj.ID);
                            sqlCommand.Parameters.AddWithValue("CSTATUS", obj.Content);
                            sqlCommand.ExecuteNonQuery();
                        }
                        sqlCommand.Connection.Close();
                    }
                }
            }
            else
            {
                throw new InvalidConnectionStringException("Connection string is empty");
            }

            // Refresh cache from database
            _refreshCache();

        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.Internal
{
    public class InternalGradeLevelRepository
    {

        private const string SelectSQL = "SELECT iGradeID, cName FROM GradeLevels";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, GradeLevel> _cache = new Dictionary<int, GradeLevel>();

        private GradeLevel dataReaderToObject(SqlDataReader dataReader)
        {
            return new GradeLevel()
            {
                ID = Parsers.ParseInt(dataReader["iGradeID"].ToString().Trim()),
                Name = dataReader["cName"].ToString().Trim()
            };
        }

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, GradeLevel>();
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
                                GradeLevel parsedObject = dataReaderToObject(dataReader);
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

        public InternalGradeLevelRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _refreshCache();
        }

        public List<int> GetAllIDs()
        {
            return _cache.Keys.ToList();
        }

        public GradeLevel Get(int id)
        {
            if (_cache.ContainsKey(id))
            {
                return _cache[id];
            }
            else
            {
                return null;
            }
        }

        public List<GradeLevel> GetAll()
        {
            return _cache.Values.ToList();
        }

        public void Add(List<GradeLevel> objs)
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
                        foreach (GradeLevel obj in objs)
                        {
                            sqlCommand.CommandText = "INSERT INTO GradeLevels(iGradeID, cName) VALUES(@STATUSID,@CSTATUS)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STATUSID", obj.ID);
                            sqlCommand.Parameters.AddWithValue("CSTATUS", obj.Name);
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

        public void Update(List<GradeLevel> objs)
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
                        foreach (GradeLevel obj in objs) {
                            sqlCommand.CommandText = "UPDATE GradeLevels SET cName=@CSTATUS WHERE iGradeID=@STATUSID";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STATUSID", obj.ID);
                            sqlCommand.Parameters.AddWithValue("CSTATUS", obj.Name);
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.Internal
{
    public class InternalSchoolRepository
    {
        private const string SelectSQL = "SELECT iSchoolID, cSchoolGovID, cName FROM Schools";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, School> _cache = new Dictionary<int, School>();

        private School dataReaderToSchool(SqlDataReader dataReader)
        {
            return new School()
            {
                iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                GovernmentID = dataReader["cSchoolGovID"].ToString().Trim(),
                Name = dataReader["cName"].ToString().Trim()
            };
        }

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, School>();
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
                                School parsedSchool = dataReaderToSchool(dataReader);
                                if (parsedSchool != null)
                                {
                                    _cache.Add(parsedSchool.iSchoolID, parsedSchool);
                                }
                            }
                        }
                        sqlCommand.Connection.Close();
                    }
                }
            } else
            {
                throw new InvalidConnectionStringException("Connection string is empty");
            }
        }

        public InternalSchoolRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _refreshCache();
        }
        
        public List<int> GetAllKnownSchoolIDs()
        {
            return _cache.Keys.ToList();
        }

        public School Get(int iSchoolID)
        {
            if (_cache.ContainsKey(iSchoolID))
            {
                return _cache[iSchoolID];
            } else
            {
                return null;
            }
        }

        public List<School> GetAll()
        {
            return _cache.Values.ToList();
        }

        public void Add(School school)
        {
            // Add to database
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, School>();
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "INSERT INTO Schools(iSchoolID, cSchoolGovID, cName) VALUES(@ISCHOOLID,@GOVID,@SCNAME)";
                        sqlCommand.Parameters.AddWithValue("ISCHOOLID", school.iSchoolID);
                        sqlCommand.Parameters.AddWithValue("GOVID", school.GovernmentID);
                        sqlCommand.Parameters.AddWithValue("SCNAME", school.Name);
                        sqlCommand.Connection.Open();
                        sqlCommand.ExecuteNonQuery();
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

        public void Update(School school)
        {
            // Update database
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, School>();
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "UPDATE Schools SET cSchoolGovID=@GOVID, cName=@SCNAME WHERE iSchoolID=@ISCHOOLID";
                        sqlCommand.Parameters.AddWithValue("ISCHOOLID", school.iSchoolID);
                        sqlCommand.Parameters.AddWithValue("GOVID", school.GovernmentID);
                        sqlCommand.Parameters.AddWithValue("SCNAME", school.Name);
                        sqlCommand.Connection.Open();
                        sqlCommand.ExecuteNonQuery();
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

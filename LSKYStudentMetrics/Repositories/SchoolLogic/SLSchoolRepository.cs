using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    public class SLSchoolRepository
    {
        private const string SelectSQL = "SELECT iSchoolID, cCode, cName FROM School WHERE iDistrictID=1";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, School> _cache = new Dictionary<int, School>();

        private School dataReaderToSchool(SqlDataReader dataReader)
        {
            return new School()
            {
                iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                GovernmentID = dataReader["cCode"].ToString().Trim(),
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
            }
            else
            {
                throw new InvalidConnectionStringException("Connection string is empty");
            }
        }

        public SLSchoolRepository(string SQLConnectionString)
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
            }
            else
            {
                return null;
            }
        }

        public List<School> GetAll()
        {
            return _cache.Values.ToList();
        }
    }
}

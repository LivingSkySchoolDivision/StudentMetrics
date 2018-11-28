using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    public class SLGradeLevelRepository
    {
        private const string SelectSQL = "SELECT iGradesID, cName FROM Grades";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, GradeLevel> _cache = new Dictionary<int, GradeLevel>();

        private GradeLevel dataReaderToObject(SqlDataReader dataReader)
        {
            return new GradeLevel()
            {
                ID = Parsers.ParseInt(dataReader["iGradesID"].ToString().Trim()),
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

        public SLGradeLevelRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _refreshCache();
        }

        public List<int> GetAllIDs()
        {
            return _cache.Keys.ToList();
        }

        public GradeLevel Get(int iAbsenceStatusID)
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

        public List<GradeLevel> GetAll()
        {
            return _cache.Values.ToList();
        }
    }
}

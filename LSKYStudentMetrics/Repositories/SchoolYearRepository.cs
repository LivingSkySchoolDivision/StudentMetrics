using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class SchoolYearRepository
    {
        private const string sql = "SELECT * FROM SchoolYears;";

        private Dictionary<int, SchoolYear> _cacheByID = new Dictionary<int, SchoolYear>();

        private SchoolYear dataReaderToSchoolYear(SqlDataReader dataReader)
        {
            return new SchoolYear()
            {
                ID = Parsers.ParseInt(dataReader["iSchoolYearID"].ToString()),
                Name = dataReader["cName"].ToString().Trim(),
                Starts = Parsers.ParseDate(dataReader["DateStarts"].ToString()),
                Ends = Parsers.ParseDate(dataReader["DateEnds"].ToString())
            };
        }

        public SchoolYearRepository(string SQLConnectionString)
        {
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = sql;

                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            SchoolYear parsedSY = dataReaderToSchoolYear(dataReader);
                            if (parsedSY != null)
                            {
                                _cacheByID.Add(parsedSY.ID, parsedSY);                                
                            }                            
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }
        }

        public SchoolYear Get(int id)
        {
            if (_cacheByID.ContainsKey(id))
            {
                return _cacheByID[id];
            }
            return null;
        }

        public SchoolYear Get(string name)
        {
            foreach(SchoolYear sy in _cacheByID.Values)
            {
                if (sy.Name.Equals(name))
                {
                    return sy;
                }
            }

            return null;
        }

    }
}

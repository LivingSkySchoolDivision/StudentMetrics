using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    class SLTermRepository
    {
        private string _SQLConnectionString = string.Empty;
        private Dictionary<int, Term> _cache;
        private SLTrackRepository _trackRepo;
        private const string selectSQL = "SELECT iTermID, iTrackID, dStartDate, dEndDate, cName, iSchoolID FROM Term";

        private Term dataReaderToObject(SqlDataReader dataReader)
        {
            Track track = _trackRepo.Get(Parsers.ParseInt(dataReader["iTrackID"].ToString().Trim()));
            if (track != null)
            {
                return new Term()
                {
                    ID = Parsers.ParseInt(dataReader["iTermID"].ToString().Trim()),
                    iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                    Name = dataReader["cName"].ToString().Trim(),
                    Track = track,
                    Starts = Parsers.ParseDate(dataReader["dStartDate"].ToString().Trim()),
                    Ends = Parsers.ParseDate(dataReader["dEndDate"].ToString().Trim())
                };
            }
            else
            {
                return null;
            }
        }

        public SLTermRepository(string SQLConnectionString)
        {
            this._cache = new Dictionary<int, Term>();
            this._SQLConnectionString = SQLConnectionString;
            this._trackRepo = new SLTrackRepository(SQLConnectionString);

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
                            Term t = dataReaderToObject(dataReader);
                            if (t != null)
                            {
                                _cache.Add(t.ID, t);
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }
        }


        public Term Get(int iTermID)
        {
            if (_cache.ContainsKey(iTermID))
            {
                return _cache[iTermID];
            } else
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    class SLTrackScheduleRepository
    {

        private const string _selectSQL = "SELECT iTrackID, dDate, cDayNumber, lAvailable FROM Calendar";
        private Dictionary<int, TrackSchedule> _cache = new Dictionary<int, TrackSchedule>();
        private string SQLConnectionString = string.Empty;

        public SLTrackScheduleRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;

            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, TrackSchedule>();
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = _selectSQL;
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                DateTime parsedDate = Parsers.ParseDate(dataReader["dDate"].ToString().Trim());
                                int trackID = Parsers.ParseInt(dataReader["iTrackID"].ToString().Trim());
                                bool available = Parsers.ParseBool(dataReader["lAvailable"].ToString().Trim());
                                string dayNum = dataReader["cDayNumber"].ToString().Trim();

                                if (trackID > 0)
                                {
                                    if (!_cache.ContainsKey(trackID))
                                    {
                                        _cache.Add(trackID, new TrackSchedule());
                                    }
                                    _cache[trackID].AddScheduleDay(parsedDate, dayNum);
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

        public TrackSchedule Get(int iTrackID)
        {
            if (_cache.ContainsKey(iTrackID))
            {
                return _cache[iTrackID];
            } else
            {
                return new TrackSchedule();
            }
        }
        
    }
}

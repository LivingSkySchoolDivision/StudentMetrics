using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    public class SLTrackRepository
    {
        private readonly SLTrackScheduleRepository _scheduleRepo;
        private string SQLConnectionString = string.Empty;
        private string _sqlSelect = "SELECT iTrackID, dStartDate, dEndDate, lDaily, iSchoolID, iDaysInCycle, iBlocksPerDay, iDailyBlocksPerDay FROM Track";
        private Dictionary<int, Track> _cache = new Dictionary<int, Track>();


        private Track dataReaderToTrack(SqlDataReader dataReader)
        {
            return new Track()
            {
                iTrackID = Parsers.ParseInt(dataReader["iTrackID"].ToString().Trim()),
                isDaily = Parsers.ParseBool(dataReader["lDaily"].ToString().Trim()),
                Starts = Parsers.ParseDate(dataReader["dStartDate"].ToString().Trim()),
                Ends = Parsers.ParseDate(dataReader["dEndDate"].ToString().Trim()),
                Schedule = _scheduleRepo.Get(Parsers.ParseInt(dataReader["iTrackID"].ToString().Trim()))
            };
        }

        public SLTrackRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _scheduleRepo = new SLTrackScheduleRepository(SQLConnectionString);
            _cache = new Dictionary<int, Track>();

            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = _sqlSelect;
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                Track t = dataReaderToTrack(dataReader);
                                if (t != null)
                                {
                                    _cache.Add(t.iTrackID, t);
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

        public List<Track> GetAll()
        {
            return _cache.Values.ToList();
        }
    }
}

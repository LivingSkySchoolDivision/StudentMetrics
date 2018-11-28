using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    public class SLTrackRepository
    {
        private readonly SLTrackScheduleRepository _scheduleRepo;
        private string SQLConnectionString = string.Empty;
        private string _sqlSelect = "SELECT iTrackID, dStartDate, dEndDate, lDaily, iSchoolID, iDaysInCycle, iBlocksPerDay, iDailyBlocksPerDay FROM Track";
        private Dictionary<int, Track> _cache = new Dictionary<int, Track>();

        // Dictionary<student ID, trackID>
        private Dictionary<int, int> _studentTrackMappings = new Dictionary<int, int>();
        
        private Track dataReaderToTrack(SqlDataReader dataReader)
        {
            return new Track()
            {
                iTrackID = Parsers.ParseInt(dataReader["iTrackID"].ToString().Trim()),
                isDaily = Parsers.ParseBool(dataReader["lDaily"].ToString().Trim()),
                Starts = Parsers.ParseDate(dataReader["dStartDate"].ToString().Trim()),
                Ends = Parsers.ParseDate(dataReader["dEndDate"].ToString().Trim()),
                Schedule = _scheduleRepo.Get(Parsers.ParseInt(dataReader["iTrackID"].ToString().Trim())),
                DailyBlocksPerDay = Parsers.ParseInt(dataReader["iDailyBlocksPerDay"].ToString().Trim()),
            };
        }

        public SLTrackRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _scheduleRepo = new SLTrackScheduleRepository(SQLConnectionString);
            _cache = new Dictionary<int, Track>();
            _studentTrackMappings = new Dictionary<int, int>();

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

                    // Get track mappings for students
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT iStudentID, iTrackID FROM Student WHERE iTrackID > 0";
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                int iTrackID = Parsers.ParseInt(dataReader["iTrackID"].ToString().Trim());
                                int iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim());

                                if (_cache.ContainsKey(iTrackID))
                                {
                                    if (!_studentTrackMappings.ContainsKey(iStudentID))
                                    {
                                        _studentTrackMappings.Add(iStudentID, iTrackID);
                                    }
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

        public Track Get(int iTrackID)
        {
            if (_cache.ContainsKey(iTrackID))
            {
                return _cache[iTrackID];
            } else
            {
                return null;
            }
        }

        public List<Track> GetAll()
        {
            return _cache.Values.ToList();
        }

        public Track GetTrackFor(Student student)
        {
            if (student != null)
            {
                return GetTrackFor(student.iStudentID);
            } else { return null; }
        }

        public Track GetTrackFor(int iStudentID)
        {
            if (_studentTrackMappings.ContainsKey(iStudentID))
            {
                return _cache[_studentTrackMappings[iStudentID]];
            } else
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.Internal
{
    public class InternalStudentExpectedAttendanceRepository
    {
        private string SQLConnectionString = string.Empty;

        // Dictionary<SchoolYear, Dictionary<Year, Dictionary<Month, Dictionary<Day, Dicionary<Student, Entry
        Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>> _cacheByDate = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>>();
        Dictionary<int, List<StudentExpectedAttendanceEntry>> _cacheByStudentID = new Dictionary<int, List<StudentExpectedAttendanceEntry>>();
        private bool _cacheLoaded = false;
        private DateTime _cacheStartDate;
        private DateTime _cacheEndDate;
                
        public InternalStudentExpectedAttendanceRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
        }

        public InternalStudentExpectedAttendanceRepository(string SQLConnectionString, DateTime startDate, DateTime endDate)
        {
            this.SQLConnectionString = SQLConnectionString;

            loadCache(startDate, endDate);
        }

        public StudentExpectedAttendance GetForStudent(int iStudentID, DateTime startDate, DateTime endDate)
        {
            return new StudentExpectedAttendance(iStudentID, GetEntriesForStudent(iStudentID, startDate, endDate));
        }

        public int RecordCount(int iSchoolYearID)
        {
            int returnMe = 0;
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "SELECT COUNT(*) as num FROM StudentExpectedBlocksPerDay WHERE iSchoolYearID=@SYID";
                    sqlCommand.Parameters.AddWithValue("SYID", iSchoolYearID);
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            returnMe = Parsers.ParseInt(dataReader["num"].ToString());
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }
            return returnMe;
        }

        public int RecordCount()
        {
            int returnMe = 0;
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "SELECT COUNT(*) as num FROM StudentExpectedBlocksPerDay";
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            returnMe = Parsers.ParseInt(dataReader["num"].ToString());
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }
            return returnMe;
        }

        private void loadCache(DateTime startDate, DateTime endDate)
        {
            _cacheByDate = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>>();
            _cacheByStudentID = new Dictionary<int, List<StudentExpectedAttendanceEntry>>();
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "SELECT * FROM StudentExpectedBlocksPerDay WHERE dDate>=@STARTDATE AND dDate<=@ENDDATE";
                    sqlCommand.Parameters.AddWithValue("STARTDATE", startDate);
                    sqlCommand.Parameters.AddWithValue("ENDDATE", endDate);
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            StudentExpectedAttendanceEntry parsedObject = dataReaderToObject(dataReader);
                            addEntryToCache(parsedObject);                            
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

            _cacheStartDate = startDate;
            _cacheEndDate = endDate;
            _cacheLoaded = true;
        }

        private void loadCache(int iSchoolYearID)
        {
            DateTime detectedEarliestDate = DateTime.MaxValue;
            DateTime detectedLatestDate = DateTime.MinValue;

            _cacheByDate = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>>();
            _cacheByStudentID = new Dictionary<int, List<StudentExpectedAttendanceEntry>>();
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "SELECT * FROM StudentExpectedBlocksPerDay WHERE iSchoolYearID=@SYID";
                    sqlCommand.Parameters.AddWithValue("SYID", iSchoolYearID);
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            StudentExpectedAttendanceEntry parsedObject = dataReaderToObject(dataReader);
                            addEntryToCache(parsedObject);
                            
                            if (parsedObject.Date < detectedEarliestDate)
                            {
                                detectedEarliestDate = parsedObject.Date;
                            }

                            if (parsedObject.Date > detectedLatestDate)
                            {
                                detectedLatestDate = parsedObject.Date;
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

            _cacheStartDate = detectedEarliestDate;
            _cacheEndDate = detectedLatestDate;
            _cacheLoaded = true;
        }
        
        private void addEntryToCache(StudentExpectedAttendanceEntry o)
        {
            if (o != null)
            {
                // Cache by date
                if (!_cacheByDate.ContainsKey(o.iSchoolYearID))
                {
                    _cacheByDate.Add(o.iSchoolYearID, new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>());
                }

                if (!_cacheByDate[o.iSchoolYearID].ContainsKey(o.Date.Year))
                {
                    _cacheByDate[o.iSchoolYearID].Add(o.Date.Year, new Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>());
                }

                if (!_cacheByDate[o.iSchoolYearID][o.Date.Year].ContainsKey(o.Date.Month))
                {
                    _cacheByDate[o.iSchoolYearID][o.Date.Year].Add(o.Date.Month, new Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>());
                }

                if (!_cacheByDate[o.iSchoolYearID][o.Date.Year][o.Date.Month].ContainsKey(o.Date.Day))
                {
                    _cacheByDate[o.iSchoolYearID][o.Date.Year][o.Date.Month].Add(o.Date.Day, new Dictionary<int, StudentExpectedAttendanceEntry>());
                }

                if (!_cacheByDate[o.iSchoolYearID][o.Date.Year][o.Date.Month][o.Date.Day].ContainsKey(o.iStudentID))
                {
                    _cacheByDate[o.iSchoolYearID][o.Date.Year][o.Date.Month][o.Date.Day].Add(o.iStudentID, o);
                }

                // Cache by student ID
                if (!_cacheByStudentID.ContainsKey(o.iStudentID))
                {
                    _cacheByStudentID.Add(o.iStudentID, new List<StudentExpectedAttendanceEntry>());
                }
                _cacheByStudentID[o.iStudentID].Add(o);
            }
        }
        
        public StudentExpectedAttendanceEntry Get(int iStudentID, int iSchoolYearID, DateTime day)
        {
            // Do some fancy caching magic here
            // We are probably only Getting an individual entry if we're performing a sync, where we'll need the entire school year
            // If the cache for the given school year is not yet loaded, load it
            // Do the rest of the checking and getting from the cache

            if (!_cacheLoaded)
            {
                loadCache(iSchoolYearID);
            } else
            {
                if (day > _cacheEndDate)
                {
                    throw new Exception("Attempted to load date range outside cache when caching was enabled.");
                }
            }

            // Brute force it, for speed
            try
            {
                return _cacheByDate[iSchoolYearID][day.Year][day.Month][day.Day][iStudentID];
            }
            catch { }

            return null;
        }

        public StudentExpectedAttendanceEntry Get(int iStudentID, int iSchoolYearID, int iYear, int iMonth, int iDay)
        {
            return Get(iStudentID, iSchoolYearID, new DateTime(iYear, iMonth, iDay));
        }

        private StudentExpectedAttendanceEntry dataReaderToObject(SqlDataReader dataReader)
        {
            return new StudentExpectedAttendanceEntry()
            {
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString()),
                iSchoolYearID = Parsers.ParseInt(dataReader["iSchoolYearID"].ToString()),
                BlocksToday = Parsers.ParseInt(dataReader["iBlocksToday"].ToString()),
                Date = Parsers.ParseDate(dataReader["dDate"].ToString())
            };
        }

        public List<StudentExpectedAttendanceEntry> GetEntriesForStudent(int iStudentID, DateTime startDate, DateTime endDate)
        {
            try
            {
                return _cacheByStudentID[iStudentID].Where(x => x.Date <= endDate && x.Date >= startDate).ToList();
            }
            catch
            {
                return new List<StudentExpectedAttendanceEntry>();
            }
        }



        public void Add(List<StudentExpectedAttendanceEntry> objs)
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
                        foreach (StudentExpectedAttendanceEntry obj in objs)
                        {
                            sqlCommand.CommandText = "INSERT INTO StudentExpectedBlocksPerDay(iStudentID, iSchoolYearID, iBlocksToday, dDate, iYear, iMonthNumber, iDay) VALUES(@STUDID, @SCHOOLYEAR, @BLOCKSTODAY, @DDATE, @IYEAR, @IMONTH, @IDAY)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STUDID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLYEAR", obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("BLOCKSTODAY", obj.BlocksToday);
                            sqlCommand.Parameters.AddWithValue("DDATE", obj.Date);
                            sqlCommand.Parameters.AddWithValue("IYEAR", obj.Date.Year);
                            sqlCommand.Parameters.AddWithValue("IMONTH", obj.Date.Month);
                            sqlCommand.Parameters.AddWithValue("IDAY", obj.Date.Day);
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
        }

        public void Update(List<StudentExpectedAttendanceEntry> objs)
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
                        foreach (StudentExpectedAttendanceEntry obj in objs)
                        {
                            sqlCommand.CommandText = "UPDATE StudentExpectedBlocksPerDay SET iBlocksToday=@BLOCKSTODAY WHERE iStudentID=@STUDID AND iSchoolYearID=@SCHOOLYEAR AND dDate=@DDATE";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STUDID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLYEAR", obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("BLOCKSTODAY", obj.BlocksToday);
                            sqlCommand.Parameters.AddWithValue("DDATE", obj.Date);
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
        }
    }
}

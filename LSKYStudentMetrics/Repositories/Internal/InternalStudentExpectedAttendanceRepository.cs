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

        //List<StudentExpectedAttendanceEntry> _allEntriesCache = new List<StudentExpectedAttendanceEntry>();
                         
        public InternalStudentExpectedAttendanceRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
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

        public StudentExpectedAttendanceEntry Get(int iStudentID, int iSchoolYearID, CalendarDay day)
        {
            return Get(iStudentID, iSchoolYearID, (DateTime)day);
        }

        // Yes this is horrific, but it's fast... I hope
        // Dictionary<SchoolYear, Dictionary<Year, Dictionary<Month, Dictionary<Day, Dicionary<Student, Entry
        Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>> _cache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>>();

        private void loadCacheForSchoolYear(int iSchoolYearID)
        {
            if (_cache.ContainsKey(iSchoolYearID))
            {
                _cache.Remove(iSchoolYearID);                
            }
            _cache.Add(iSchoolYearID, new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>>());
            
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
                            StudentExpectedAttendanceEntry o = dataReaderToObject(dataReader);
                            if (o != null)
                            {
                                // Insert into the cache
                                if (!_cache[iSchoolYearID].ContainsKey(o.Date.Year))
                                {
                                    _cache[iSchoolYearID].Add(o.Date.Year, new Dictionary<int, Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>>());
                                }

                                if (!_cache[iSchoolYearID][o.Date.Year].ContainsKey(o.Date.Month))
                                {
                                    _cache[iSchoolYearID][o.Date.Year].Add(o.Date.Month, new Dictionary<int, Dictionary<int, StudentExpectedAttendanceEntry>>());
                                }

                                if (!_cache[iSchoolYearID][o.Date.Year][o.Date.Month].ContainsKey(o.Date.Day))
                                {
                                    _cache[iSchoolYearID][o.Date.Year][o.Date.Month].Add(o.Date.Day, new Dictionary<int, StudentExpectedAttendanceEntry>());
                                }

                                if (!_cache[iSchoolYearID][o.Date.Year][o.Date.Month][o.Date.Day].ContainsKey(o.iStudentID))
                                {
                                    _cache[iSchoolYearID][o.Date.Year][o.Date.Month][o.Date.Day].Add(o.iStudentID, o);
                                }
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }
        }

        public StudentExpectedAttendanceEntry Get(int iStudentID, int iSchoolYearID, DateTime day)
        {
            // Do some fancy caching magic here
            // We are probably only Getting an individual entry if we're performing a sync, where we'll need the entire school year

            // If the cache for the given school year is not yet loaded, load it

            // Do the rest of the checking and getting from the cache

            if (!_cache.ContainsKey(iSchoolYearID))
            {
                loadCacheForSchoolYear(iSchoolYearID);
            }

            // Brute force it, for speed
            try
            {
                return _cache[iSchoolYearID][day.Year][day.Month][day.Day][iStudentID];
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
            List<StudentExpectedAttendanceEntry> returnMe = new List<StudentExpectedAttendanceEntry>();

            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "SELECT * FROM StudentExpectedBlocksPerDay WHERE iStudentID=@STUDID AND dDate>=@STARTDATE AND dDate<=@ENDDATE";
                    sqlCommand.Parameters.AddWithValue("STARTDATE", startDate);
                    sqlCommand.Parameters.AddWithValue("ENDDATE", endDate);
                    sqlCommand.Parameters.AddWithValue("STUDID", iStudentID);
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            StudentExpectedAttendanceEntry parsedObject = dataReaderToObject(dataReader);
                            if (parsedObject != null)
                            {
                                returnMe.Add(parsedObject);
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

            return returnMe;
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

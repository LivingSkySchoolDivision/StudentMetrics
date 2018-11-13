using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.Internal
{
    public class InternalStudentExpectedAttendanceRepository
    {
        private const string _selectSQL = "SELECT * FROM StudentExpectedBlocksPerDay";
        private string SQLConnectionString = string.Empty;

        //List<StudentExpectedAttendanceEntry> _allEntriesCache = new List<StudentExpectedAttendanceEntry>();
          
        // This is going to be ugly, but it needs to be fast
        // School year, year, month, day, studentID, blockstoday
        Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>> _cache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>();
        Dictionary<int, int> _countsBySchoolYear = new Dictionary<int, int>();
        
        private int _totalRecords = 0;

        public int TotalRecords()
        {
            return _totalRecords;
        }

        public int TotalRecords(int iSchoolYear)
        {
            return _countsBySchoolYear.ContainsKey(iSchoolYear) ? _countsBySchoolYear[iSchoolYear] : 0;
        }

        public InternalStudentExpectedAttendanceRepository(string SQLConnectionString) : this(SQLConnectionString, 0) { }

        public InternalStudentExpectedAttendanceRepository(string SQLConnectionString, int iSchoolYearID)
        {
            this.SQLConnectionString = SQLConnectionString;
            _totalRecords = 0;
            
            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    if (iSchoolYearID == 0)
                    {
                        sqlCommand.CommandText = _selectSQL;
                    } else
                    {
                        sqlCommand.CommandText = _selectSQL + " WHERE iSchoolYearID=@SYID";
                        sqlCommand.Parameters.Clear();
                        sqlCommand.Parameters.AddWithValue("SYID", iSchoolYearID);
                    }
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            addToCache(dataReader);                            
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }
        }

        private void addToCache(SqlDataReader dataReader)
        {
            int _iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString());
            int _iSchoolYearID = Parsers.ParseInt(dataReader["iSchoolYearID"].ToString());
            int _year = Parsers.ParseInt(dataReader["iYear"].ToString());
            int _month = Parsers.ParseInt(dataReader["iMonthNumber"].ToString());
            int _day = Parsers.ParseInt(dataReader["iDay"].ToString());
            int _blocksToday = Parsers.ParseInt(dataReader["iBlocksToday"].ToString());

            addToCache(_iSchoolYearID, _year, _month, _day, _iStudentID, _blocksToday);            
        }

        private void addToCache(StudentExpectedAttendanceEntry entry)
        {   
            addToCache(entry.iSchoolYearID, entry.Year, entry.Month, entry.Day, entry.iStudentID, entry.BlocksToday);
        }

        private void addToCache(int schoolYear, int year, int month, int day, int studentID, int blocksPerDay)
        {
            if ((studentID != 0) && (schoolYear != 0) && (year != 0) && (month != 0) && (day != 0) && (blocksPerDay != 0))
            {
                // School Year ID
                if (!_cache.ContainsKey(schoolYear))
                {
                    _cache.Add(schoolYear, new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>());
                }

                // Year
                if (!_cache[schoolYear].ContainsKey(year))
                {
                    _cache[schoolYear].Add(year, new Dictionary<int, Dictionary<int, Dictionary<int, int>>>());
                }

                // Month
                if (!_cache[schoolYear][year].ContainsKey(month))
                {
                    _cache[schoolYear][year].Add(month, new Dictionary<int, Dictionary<int, int>>());
                }

                // Day
                if (!_cache[schoolYear][year][month].ContainsKey(day))
                {
                    _cache[schoolYear][year][month].Add(day, new Dictionary<int, int>());
                }

                // Student ID
                if (!_cache[schoolYear][year][month][day].ContainsKey(studentID))
                {
                    _cache[schoolYear][year][month][day].Add(studentID, 0);
                }

                _cache[schoolYear][year][month][day][studentID] = blocksPerDay;
                _totalRecords++;

                if (!_countsBySchoolYear.ContainsKey(schoolYear))
                {
                    _countsBySchoolYear.Add(schoolYear, 0);
                }
                _countsBySchoolYear[schoolYear]++;
            }
        }
        
        public StudentExpectedAttendance GetForStudent(int iStudentID)
        {
            return new StudentExpectedAttendance(iStudentID, GetEntriesFor(iStudentID));
        }

                
        public StudentExpectedAttendanceEntry Get(int iStudentID, int iSchoolYearID, int iYear, int iMonth, int iDay)
        {
            int bpd = 0;
            try
            {
                bpd = _cache?[iSchoolYearID]?[iYear]?[iMonth]?[iDay]?[iStudentID] ?? 0;
            }
            catch { }

            // To keep things consistent with other repositories, return null instead of zero here
            if (bpd == 0) return null;

            return new StudentExpectedAttendanceEntry()
            {
                iStudentID = iStudentID,
                iSchoolYearID = iSchoolYearID,
                Year = iYear,
                Month = iMonth,
                Day = iDay,
                BlocksToday = bpd
            };            
        }

        public List<StudentExpectedAttendanceEntry> GetEntriesFor(int iStudentID)
        {
            List<StudentExpectedAttendanceEntry> returnMe = new List<StudentExpectedAttendanceEntry>();

            foreach (int schoolYear in _cache.Keys)
            {
                foreach(int year in _cache[schoolYear].Keys)
                {
                    foreach (int month in _cache[schoolYear][year].Keys)
                    {
                        foreach(int day in _cache[schoolYear][year][month].Keys)
                        {
                            if (_cache[schoolYear][year][month][day].ContainsKey(iStudentID))
                            {
                                returnMe.Add(new StudentExpectedAttendanceEntry()
                                {
                                    iStudentID = iStudentID,
                                    iSchoolYearID = schoolYear,
                                    Year = year,
                                    Month = month,
                                    Day = day,
                                    BlocksToday = _cache[schoolYear][year][month][day][iStudentID]
                                });
                            }
                        }
                    }
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
                            sqlCommand.CommandText = "INSERT INTO StudentExpectedBlocksPerDay(iStudentID, iSchoolYearID, iYear, iMonthNumber, iDay, iBlocksToday) VALUES(@STUDID, @SCHOOLYEAR, @YEARNUM, @MONTHNUM, @DAYNUM, @BLOCKSTODAY)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STUDID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLYEAR", obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("YEARNUM", obj.Year);
                            sqlCommand.Parameters.AddWithValue("MONTHNUM", obj.Month);
                            sqlCommand.Parameters.AddWithValue("DAYNUM", obj.Day);
                            sqlCommand.Parameters.AddWithValue("BLOCKSTODAY", obj.BlocksToday);
                            sqlCommand.ExecuteNonQuery();


                            // Also add to the internal cache
                            addToCache(obj);
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
                            sqlCommand.CommandText = "UPDATE StudentExpectedBlocksPerDay SET iBlocksToday=@BLOCKSTODAY WHERE iStudentID=@STUDID AND iSchoolYearID=@SCHOOLYEAR AND iYear=@YEARNUM AND iMonthNumber=@MONTHNUM AND iDay=@DAYNUM";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STUDID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLYEAR", obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("YEARNUM", obj.Year);
                            sqlCommand.Parameters.AddWithValue("MONTHNUM", obj.Month);
                            sqlCommand.Parameters.AddWithValue("DAYNUM", obj.Day);
                            sqlCommand.Parameters.AddWithValue("BLOCKSTODAY", obj.BlocksToday);
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

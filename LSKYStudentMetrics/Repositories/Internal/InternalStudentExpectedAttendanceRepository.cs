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

        // This should be rewritten with a dictionary to see if it's any faster because holy shit is this not efficient
        List<StudentExpectedAttendanceEntry> _allEntriesCache = new List<StudentExpectedAttendanceEntry>();

        public InternalStudentExpectedAttendanceRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;

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
                            StudentExpectedAttendanceEntry parsedObject = dataReaderToObject(dataReader);
                            if (parsedObject != null)
                            {
                                _allEntriesCache.Add(parsedObject);
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

        }

        private StudentExpectedAttendanceEntry dataReaderToObject(SqlDataReader dataReader)
        {
            return new StudentExpectedAttendanceEntry()
            {
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString()),
                iSchoolYearID = Parsers.ParseInt(dataReader["iSchoolYearID"].ToString()),
                Year = Parsers.ParseInt(dataReader["iYear"].ToString()),
                Month = Parsers.ParseInt(dataReader["iMonthNumber"].ToString()),
                Day = Parsers.ParseInt(dataReader["iDay"].ToString()),
                BlocksToday = Parsers.ParseInt(dataReader["iBlocksToday"].ToString())
            };
        }      
        
        
        public StudentExpectedAttendance GetForStudent(int iStudentID)
        {
            return new StudentExpectedAttendance(iStudentID, _allEntriesCache.Where(entry => entry.iStudentID == iStudentID).ToList());            
        }

                
        public StudentExpectedAttendanceEntry Get(int iStudentID, int iSchoolYearID, int iYear, int iMonth, int iDay)
        {
            return _allEntriesCache.Where(x => x.iStudentID == iStudentID && x.iSchoolYearID == iSchoolYearID && x.Year == iYear && x.Month == iMonth && x.Day == iDay).FirstOrDefault() ?? null;
        }


        public List<StudentExpectedAttendanceEntry> GetAllEntriesForYear(int schoolYearID)
        {
            return _allEntriesCache.Where(e => e.iSchoolYearID == schoolYearID).ToList();
        }

        public List<StudentExpectedAttendanceEntry> GetEntriesFor(int iStudentID)
        {
            return _allEntriesCache.Where(e => e.iStudentID == iStudentID).ToList();
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

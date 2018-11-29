using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.Internal
{
    public class InternalAbsenceRepository
    {        
        private string SQLConnectionString = string.Empty;

        Dictionary<int, List<Absence>> _cacheByStudentID = new Dictionary<int, List<Absence>>();
        // So we can track how many absences the student has without loading them all
        // Mostly so we know when we need to load more data
        Dictionary<int, int> _absenceCounts = new Dictionary<int, int>();

        // Should this repo pull data from a preloaded cache, or should it pull directly from the database
        bool _cachedMode = false;
        bool _cacheLoaded = false;
        DateTime _cache_startDate;
        DateTime _cache_endDate;
                
        private InternalAbsenceReasonRepository _reasonRepo = null;
        private InternalAbsenceStatusRepository _statusRepo = null;

        private Absence dataReaderToObject(SqlDataReader dataReader)
        {
            return new Absence()
            {
                ID = Parsers.ParseInt(dataReader["iAbsenceID"].ToString().Trim()),
                Date = Parsers.ParseDate(dataReader["dDate"].ToString().Trim()),
                iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                BlockNumber = Parsers.ParseInt(dataReader["iBlockNumber"].ToString().Trim()),
                Minutes = Parsers.ParseDecimal(dataReader["iMinutes"].ToString().Trim()),
                iClassID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim()),
                iHomeRoomID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim()),
                iReasonID = Parsers.ParseInt(dataReader["iReasonID"].ToString().Trim()),
                iStatusID = Parsers.ParseInt(dataReader["iStatusID"].ToString().Trim()),
                iStaffID = Parsers.ParseInt(dataReader["iStaffID"].ToString().Trim()),
                Status = _statusRepo.Get(Parsers.ParseInt(dataReader["iStatusID"].ToString().Trim())),
                Reason = _reasonRepo.Get(Parsers.ParseInt(dataReader["iReasonID"].ToString().Trim())),
                iSchoolYearID = Parsers.ParseInt(dataReader["iSchoolYearID"].ToString().Trim())
            };
        }

        private void updateKnownAbsenceCounts()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _absenceCounts = new Dictionary<int, int>();
                _absenceCounts.Clear();

                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT iStudentID, COUNT(*) as NumRecords FROM Attendance GROUP BY iStudentID";
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                int iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString());
                                int numRecords = Parsers.ParseInt(dataReader["NumRecords"].ToString());
                                _absenceCounts.Add(iStudentID, numRecords);
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

        private bool checkIfCacheReloadIsNeeded(DateTime startDate, DateTime endDate)
        {
            if (!_cacheLoaded) { return true; }
            if (startDate != _cache_startDate) { return true; }
            if (endDate != _cache_endDate) { return true; }
            return false;
        }

        private void loadCache(DateTime startDate, DateTime endDate)
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cacheByStudentID = new Dictionary<int, List<Absence>>();
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT * FROM Attendance WHERE dDate>=@STARTDATE AND dDate<=@ENDDATE";
                        sqlCommand.Parameters.AddWithValue("STARTDATE", startDate);
                        sqlCommand.Parameters.AddWithValue("ENDDATE", endDate);
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                Absence parsedObject = dataReaderToObject(dataReader);
                                if (parsedObject != null)
                                {
                                    // Add object to the cache
                                    if (!_cacheByStudentID.ContainsKey(parsedObject.iStudentID))
                                    {
                                        _cacheByStudentID.Add(parsedObject.iStudentID, new List<Absence>());
                                    }
                                    _cacheByStudentID[parsedObject.iStudentID].Add(parsedObject);
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

            _cacheLoaded = true;
            _cache_startDate = startDate;
            _cache_endDate = endDate;
        }        

        // When getting absences for a student

        public InternalAbsenceRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            this._reasonRepo = new InternalAbsenceReasonRepository(SQLConnectionString);
            this._statusRepo = new InternalAbsenceStatusRepository(SQLConnectionString);
        }

        public InternalAbsenceRepository(string SQLConnectionString, DateTime startDate, DateTime endDate)
        {
            this._cachedMode = true;
            this.SQLConnectionString = SQLConnectionString;
            this._reasonRepo = new InternalAbsenceReasonRepository(SQLConnectionString);
            this._statusRepo = new InternalAbsenceStatusRepository(SQLConnectionString);
            updateKnownAbsenceCounts();
        }

        public List<int> GetAllIDs()
        {
            List<int> returnMe = new List<int>();
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "Select iAbsenceID from Attendance";
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                int parsedID = Parsers.ParseInt(dataReader["iAbsenceID"].ToString().Trim());
                                if (parsedID > 0)
                                {
                                    if (!returnMe.Contains(parsedID))
                                    {
                                        returnMe.Add(parsedID);
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
            return returnMe;
        }
        
        /// <summary>
        /// Gets all absences from the internal database. This can take a very long time.
        /// </summary>
        /// <returns></returns>
        [Obsolete("This will take a very long time. Consider getting a specific date range instead.")]
        public List<Absence> GetAll()
        {
            List<Absence> returnMe = new List<Absence>();
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT * FROM Attendance;";
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                Absence parsedObject = dataReaderToObject(dataReader);
                                if (parsedObject != null)
                                {
                                    returnMe.Add(parsedObject);
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
            return returnMe;
        }

        public List<Absence> GetForSchoolYear(int schoolYearID)
        {
            List<Absence> returnMe = new List<Absence>();
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT * FROM Attendance WHERE iSchoolYearID=@SYID;";
                        sqlCommand.Parameters.AddWithValue("SYID", schoolYearID);
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                Absence parsedObject = dataReaderToObject(dataReader);
                                if (parsedObject != null)
                                {
                                    returnMe.Add(parsedObject);
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
            return returnMe;
        }
                

        public List<Absence> GetForStudent(int iStudentID, DateTime startDate, DateTime endDate)
        {
            if ((!_cacheLoaded) || (checkIfCacheReloadIsNeeded(startDate, endDate)))
            {
                loadCache(startDate, endDate);
            }

            try
            {
                return _cacheByStudentID[iStudentID];
            }
            catch { }

            return new List<Absence>();
        }



        public void Add(List<Absence> objs)
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
                        foreach (Absence obj in objs)
                        {
                            sqlCommand.CommandText = "INSERT INTO Attendance(iSchoolYearID,iAbsenceID,dDate,iSchoolID,iStudentID,iBlockNumber,iMinutes,iClassID,iReasonID,iStatusID,iStaffID) VALUES(@ISCHOOLYEARID,@IABSENCEID,@DDATE,@ISCHOOLID,@ISTUDENTID,@IBLOCKNUMBER,@IMINUTES,@ICLASSID,@IREASONID,@ISTATUSID,@ISTAFFID)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("ISCHOOLYEARID",obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("IABSENCEID",obj.ID);
                            sqlCommand.Parameters.AddWithValue("DDATE",obj.Date.ToDatabaseSafeDate());
                            sqlCommand.Parameters.AddWithValue("ISCHOOLID",obj.iSchoolID);
                            sqlCommand.Parameters.AddWithValue("ISTUDENTID",obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("IBLOCKNUMBER",obj.BlockNumber);
                            sqlCommand.Parameters.AddWithValue("IMINUTES",obj.Minutes);
                            sqlCommand.Parameters.AddWithValue("ICLASSID",obj.iClassID);
                            sqlCommand.Parameters.AddWithValue("IREASONID",obj.iReasonID);
                            sqlCommand.Parameters.AddWithValue("ISTATUSID",obj.iStatusID);
                            sqlCommand.Parameters.AddWithValue("ISTAFFID",obj.iStaffID);
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

        public void Update(List<Absence> objs)
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
                        foreach (Absence obj in objs)
                        {
                            sqlCommand.CommandText = "UPDATE Attendance SET iSchoolYearID=@ISCHOOLYEARID, dDate=@DDATE, iSchoolID=@ISCHOOLID, iStudentID=@ISTUDENTID, iBlockNumber=@IBLOCKNUMBER, iMinutes=@IMINUTES, iClassID=@ICLASSID, iReasonID=@IREASONID, iStatusID=@ISTATUSID, iStaffID=@ISTAFFID WHERE iAbsenceID=@IABSENCEID";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("ISCHOOLYEARID", obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("IABSENCEID", obj.ID);
                            sqlCommand.Parameters.AddWithValue("DDATE", obj.Date.ToDatabaseSafeDate());
                            sqlCommand.Parameters.AddWithValue("ISCHOOLID", obj.iSchoolID);
                            sqlCommand.Parameters.AddWithValue("ISTUDENTID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("IBLOCKNUMBER", obj.BlockNumber);
                            sqlCommand.Parameters.AddWithValue("IMINUTES", obj.Minutes);
                            sqlCommand.Parameters.AddWithValue("ICLASSID", obj.iClassID);
                            sqlCommand.Parameters.AddWithValue("IREASONID", obj.iReasonID);
                            sqlCommand.Parameters.AddWithValue("ISTATUSID", obj.iStatusID);
                            sqlCommand.Parameters.AddWithValue("ISTAFFID", obj.iStaffID);
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

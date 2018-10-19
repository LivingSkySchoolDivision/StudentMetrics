using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.Internal
{
    public class InternalAbsenceRepository
    {
        private const string SelectSQL = "SELECT * FROM Attendance";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, Absence> _cache = new Dictionary<int, Absence>();

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
                LateMinutes = Parsers.ParseDecimal(dataReader["iMinutes"].ToString().Trim()),
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

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, Absence>();
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = SelectSQL;
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                Absence parsedObject = dataReaderToObject(dataReader);
                                if (parsedObject != null)
                                {
                                    _cache.Add(parsedObject.ID, parsedObject);
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

        public InternalAbsenceRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            this._reasonRepo = new InternalAbsenceReasonRepository(SQLConnectionString);
            this._statusRepo = new InternalAbsenceStatusRepository(SQLConnectionString);
            _refreshCache();
        }

        public List<int> GetAllIDs()
        {
            return _cache.Keys.ToList();
        }

        public Absence Get(int iAbsenceStatusID)
        {
            if (_cache.ContainsKey(iAbsenceStatusID))
            {
                return _cache[iAbsenceStatusID];
            }
            else
            {
                return null;
            }
        }

        public List<Absence> GetAll()
        {
            return _cache.Values.ToList();
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
                            sqlCommand.CommandText = "INSERT INTO Attendance(iSchoolYearID,iAbsenceID,dDate,iSchoolID,iStudentID,lIsAbsence,lIsDaily,iBlockNumber,iMinutes,iClassID,iReasonID,iStatusID,iStaffID) VALUES(@ISCHOOLYEARID,@IABSENCEID,@DDATE,@ISCHOOLID,@ISTUDENTID,@LISABSENCE,@LISDAILY,@IBLOCKNUMBER,@IMINUTES,@ICLASSID,@IREASONID,@ISTATUSID,@ISTAFFID)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("ISCHOOLYEARID",obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("IABSENCEID",obj.ID);
                            sqlCommand.Parameters.AddWithValue("DDATE",obj.Date);
                            sqlCommand.Parameters.AddWithValue("ISCHOOLID",obj.iSchoolID);
                            sqlCommand.Parameters.AddWithValue("ISTUDENTID",obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("LISABSENCE",0);
                            sqlCommand.Parameters.AddWithValue("LISDAILY",0);
                            sqlCommand.Parameters.AddWithValue("IBLOCKNUMBER",obj.BlockNumber);
                            sqlCommand.Parameters.AddWithValue("IMINUTES",obj.LateMinutes);
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

            // Refresh cache from database
            _refreshCache();
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
                            sqlCommand.CommandText = "UPDATE Attendance SET iSchoolYearID=@ISCHOOLYEARID, dDate=@DDATE, iSchoolID=@ISCHOOLID, iStudentID=@ISTUDENTID, lIsAbsence=@LISABSENCE, lIsDaily=@LISDAILY, iBlockNumber=@IBLOCKNUMBER, iMinutes=@IMINUTES, iClassID=@ICLASSID, iReasonID=@IREASONID, iStatusID=@ISTATUSID, iStaffID=@ISTAFFID WHERE iAbsenceID=@IABSENCEID";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("ISCHOOLYEARID", obj.iSchoolYearID);
                            sqlCommand.Parameters.AddWithValue("IABSENCEID", obj.ID);
                            sqlCommand.Parameters.AddWithValue("DDATE", obj.Date);
                            sqlCommand.Parameters.AddWithValue("ISCHOOLID", obj.iSchoolID);
                            sqlCommand.Parameters.AddWithValue("ISTUDENTID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("LISABSENCE", 0);
                            sqlCommand.Parameters.AddWithValue("LISDAILY", 0);
                            sqlCommand.Parameters.AddWithValue("IBLOCKNUMBER", obj.BlockNumber);
                            sqlCommand.Parameters.AddWithValue("IMINUTES", obj.LateMinutes);
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

            // Refresh cache from database
            _refreshCache();

        }
    }
}

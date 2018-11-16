using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    public class SLAbsenceRepository
    {
        private const string SelectSQL = "SELECT * FROM Attendance";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, Absence> _cache = new Dictionary<int, Absence>();

        private SchoolYear schoolYear = null;
        private SLAbsenceReasonRepository _reasonRepo = null;
        private SLAbsenceStatusRepository _statusRepo = null;

        private Absence dataReaderToObject(SqlDataReader dataReader)
        {
            return new Absence()
            {
                ID = Parsers.ParseInt(dataReader["iAttendanceID"].ToString().Trim()),
                Date = Parsers.ParseDate(dataReader["dDate"].ToString().Trim()),
                iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                BlockNumber = Parsers.ParseInt(dataReader["iBlockNumber"].ToString().Trim()),
                Minutes = Parsers.ParseDecimal(dataReader["iMinutes"].ToString().Trim()),
                iClassID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim()),
                iHomeRoomID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim()),
                iReasonID = Parsers.ParseInt(dataReader["iAttendanceReasonsID"].ToString().Trim()),
                iStatusID = Parsers.ParseInt(dataReader["iAttendanceStatusID"].ToString().Trim()),
                iStaffID = Parsers.ParseInt(dataReader["iStaffID"].ToString().Trim()),
                Status = _statusRepo.Get(Parsers.ParseInt(dataReader["iAttendanceStatusID"].ToString().Trim())),
                Reason = _reasonRepo.Get(Parsers.ParseInt(dataReader["iAttendanceReasonsID"].ToString().Trim())),
                iSchoolYearID = this.schoolYear.ID
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

        public SLAbsenceRepository(string SQLConnectionString, SchoolYear schoolYear)
        {
            this.SQLConnectionString = SQLConnectionString;
            this._reasonRepo = new SLAbsenceReasonRepository(SQLConnectionString);
            this._statusRepo = new SLAbsenceStatusRepository(SQLConnectionString);
            this.schoolYear = schoolYear ?? throw new InvalidSchoolYearException("School year cannot be null");
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
    }
}

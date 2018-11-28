using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.Internal
{
    public class InternalStudentSchoolEnrolmentRepository
    {
        private const string SelectSQL = "SELECT * FROM StudentSchoolEnrolments";
        private string SQLConnectionString = string.Empty;

        Dictionary<int, StudentSchoolEnrolment> _cacheByID = new Dictionary<int, StudentSchoolEnrolment>();
        Dictionary<int, List<StudentSchoolEnrolment>> _cacheByStudent = new Dictionary<int, List<StudentSchoolEnrolment>>();

        private StudentSchoolEnrolment dataReaderToObject(SqlDataReader dataReader)
        {
            return new StudentSchoolEnrolment()
            {
                ID = Parsers.ParseInt(dataReader["enrolmentID"].ToString().Trim()),
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                InDate = Parsers.ParseDate(dataReader["InDate"].ToString().Trim()),
                OutDate = Parsers.ParseDate(dataReader["OutDate"].ToString().Trim()),
                OutsideTrackID = Parsers.ParseInt(dataReader["iOutsideTrackID"].ToString().Trim()),
                InStatus = dataReader["InStatus"].ToString().Trim(),
                OutStatus = dataReader["OutStatus"].ToString().Trim()
            };
        }

        public InternalStudentSchoolEnrolmentRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;

            _cacheByID = new Dictionary<int, StudentSchoolEnrolment>();
            _cacheByStudent = new Dictionary<int, List<StudentSchoolEnrolment>>();

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
                            StudentSchoolEnrolment parsedObject = dataReaderToObject(dataReader);
                            if (parsedObject != null)
                            {
                                if (!_cacheByID.ContainsKey(parsedObject.ID))
                                {
                                    _cacheByID.Add(parsedObject.ID, parsedObject);
                                }

                                if (!_cacheByStudent.ContainsKey(parsedObject.iStudentID))
                                {
                                    _cacheByStudent.Add(parsedObject.iStudentID, new List<StudentSchoolEnrolment>());
                                }
                                _cacheByStudent[parsedObject.iStudentID].Add(parsedObject);
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

        }

        public StudentSchoolEnrolment Get(int id)
        {
            return _cacheByID.ContainsKey(id) ? _cacheByID[id] : null;
        }

        public List<StudentSchoolEnrolment> GetForStudent(int iStudentID)
        {
            return _cacheByStudent.ContainsKey(iStudentID) ? _cacheByStudent[iStudentID] : new List<StudentSchoolEnrolment>();
        }

        public List<StudentSchoolEnrolment> GetAll()
        {
            return _cacheByID.Values.ToList();
        }

        public List<int> GetAllIDs()
        {
            return _cacheByID.Keys.ToList();
        }
        
        public void Add(List<StudentSchoolEnrolment> objs)
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
                        foreach (StudentSchoolEnrolment obj in objs)
                        {
                            sqlCommand.CommandText = "INSERT INTO StudentSchoolEnrolments(enrolmentID,iStudentID,iSchoolID,InDate,OutDate,InStatus,OutStatus,iOutsideTrackID) VALUES(@EID,@STUDID,@SCHOOLID,@INDATE,@OUTDATE,@INSTATUS,@OUTSTATUS,@OUTTRACK)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("EID",obj.ID);
                            sqlCommand.Parameters.AddWithValue("STUDID",obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLID",obj.iSchoolID);
                            sqlCommand.Parameters.AddWithValue("INDATE",obj.InDate.ToDatabaseSafeDate());
                            sqlCommand.Parameters.AddWithValue("OUTDATE",obj.OutDate.ToDatabaseSafeDate());
                            sqlCommand.Parameters.AddWithValue("INSTATUS",obj.InStatus);
                            sqlCommand.Parameters.AddWithValue("OUTSTATUS",obj.OutStatus);
                            sqlCommand.Parameters.AddWithValue("OUTTRACK",obj.OutsideTrackID);

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

        public void Update(List<StudentSchoolEnrolment> objs)
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
                        foreach (StudentSchoolEnrolment obj in objs)
                        {
                            sqlCommand.CommandText = "UPDATE StudentSchoolEnrolments SET iStudentID=@STUDID, iSchoolID=@SCHOOLID, InDate=@INDATE, OutDate=@OUTDATE, InStatus=@INSTATUS, OutStatus=@OUTSTATUS, iOutsideTrackID=@OUTTRACK WHERE enrolmentID=@EID";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("EID", obj.ID);
                            sqlCommand.Parameters.AddWithValue("STUDID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLID", obj.iSchoolID);
                            sqlCommand.Parameters.AddWithValue("INDATE", obj.InDate.ToDatabaseSafeDate());
                            sqlCommand.Parameters.AddWithValue("OUTDATE", obj.OutDate.ToDatabaseSafeDate());
                            sqlCommand.Parameters.AddWithValue("INSTATUS", obj.InStatus);
                            sqlCommand.Parameters.AddWithValue("OUTSTATUS", obj.OutStatus);
                            sqlCommand.Parameters.AddWithValue("OUTTRACK", obj.OutsideTrackID);
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

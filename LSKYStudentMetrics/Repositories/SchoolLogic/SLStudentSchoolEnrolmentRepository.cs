using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    public class SLStudentSchoolEnrolmentRepository
    {
        private const string SelectSQL = "SELECT iStudentStatusID, iStudentID, StudentStatus.iSchoolID, dInDate, dOutDate, iOutside_TrackID, LVInStatus.cName as cInStatus, LVOutStatus.cName as cOutStatus, lOutsideStatus FROM StudentStatus " +
            "LEFT OUTER JOIN LookupValues as LVInStatus ON StudentStatus.iLV_InStatusValueID=LVInStatus.iLookupValuesID " +
            "LEFT OUTER JOIN LookupValues as LVOutStatus ON StudentStatus.iLV_OutStatusValueID=LVOutStatus.iLookupValuesID ";
        private string SQLConnectionString = string.Empty;

        private StudentSchoolEnrolment dataReaderToObject(SqlDataReader dataReader)
        {
            return new StudentSchoolEnrolment()
            {
                ID = Parsers.ParseInt(dataReader["iStudentStatusID"].ToString().Trim()),
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                InDate = Parsers.ParseDate(dataReader["dInDate"].ToString().Trim()),
                OutDate = Parsers.ParseDate(dataReader["dOutDate"].ToString().Trim()),
                OutsideTrackID = Parsers.ParseInt(dataReader["iOutside_TrackID"].ToString().Trim()),
                InStatus = dataReader["cInStatus"].ToString().Trim(),
                OutStatus = dataReader["cOutStatus"].ToString().Trim(),
                BaseSchoolEnrolment = !Parsers.ParseBool(dataReader["lOutsideStatus"].ToString().Trim())
            };
        }

        public SLStudentSchoolEnrolmentRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
        }

        public List<StudentSchoolEnrolment> GetAll()
        {
            List<StudentSchoolEnrolment> returnMe = new List<StudentSchoolEnrolment>();

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
                                returnMe.Add(parsedObject);
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

            return returnMe;
        }

        public List<int> GetAllIDs()
        {
            List<int> returnMe = new List<int>();

            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "SELECT DISTINCT(iStudentStatusID) FROM StudentStatus";
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int parsedObject = Parsers.ParseInt(dataReader["iStudentStatusID"].ToString().Trim());
                            if (parsedObject != 0)
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
    }
}

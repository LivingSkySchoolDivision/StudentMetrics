using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    class SLStudentClassEnrolmentRepository
    {
        SLClassRepository _schoolClassRepository;
        private string SQLConnectionString = string.Empty;
              

        public SLStudentClassEnrolmentRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _schoolClassRepository = new SLClassRepository(SQLConnectionString);
        }

        private EnrolmentType parseEnrolmentType(string status)
        {
            if (string.IsNullOrEmpty(status)) { return EnrolmentType.Current; }
            if (status.ToLower().Equals("currently enrolled")) { return EnrolmentType.Current; }
            if (status.ToLower().Equals("completed")) { return EnrolmentType.Complete; }
            if (status.ToLower().Equals("incomplete")) { return EnrolmentType.Incomplete; }
            if (status.ToLower().Equals("withdrawn")) { return EnrolmentType.Withdrawn; }
            if (status.ToLower().Equals("no mark")) { return EnrolmentType.NoMark; }
            if (status.ToLower().Equals("standing granted")) { return EnrolmentType.StandingGranted; }
            return EnrolmentType.Unknown;
        }

        private StudentClassEnrolment dataReaderToObject(SqlDataReader dataReader)
        { 
            int studentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim());
            int classID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim());
            SchoolClass sc = _schoolClassRepository.Get(classID);

            if (sc == null)
            {
                return null;
            }

            return new StudentClassEnrolment()
            {
                iStudentID = studentID,
                iClassID = classID,
                Class = sc,
                InDate = Parsers.ParseDate(dataReader["dInDate"].ToString().Trim()),
                OutDate = Parsers.ParseDate(dataReader["dOutDate"].ToString().Trim()),
                Status = parseEnrolmentType(dataReader["enStatus"].ToString().Trim())
            };
        }
        
        public List<StudentClassEnrolment> GetForStudent(int iStudentiD)
        {
            Dictionary<int, List<StudentClassEnrolment>> returnMe = GetForStudents(new List<int>() { iStudentiD });

            if (returnMe.ContainsKey(iStudentiD))
            {
                return returnMe[iStudentiD];
            }
            else
            {
                return new List<StudentClassEnrolment>();
            }
        }

        public Dictionary<int, List<StudentClassEnrolment>> GetForStudents(List<int> iStudentIDs)
        {
            Dictionary<int, List<StudentClassEnrolment>> returnMe = new Dictionary<int, List<StudentClassEnrolment>>();

            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;

                    foreach (int studentID in iStudentIDs)
                    {
                        sqlCommand.CommandText = "SELECT " +
                                                "	iStudentID, " +
                                                "	iClassID, " +
                                                "	dInDate, " +
                                                "	dOutDate, " +
                                                "	LookupValues.cName as enStatus " +
                                                "FROM Enrollment " +
                                                "LEFT OUTER JOIN LookupValues ON Enrollment.iLV_CompletionStatusID=LookupValues.iLookupValuesID " +
                                                "WHERE iStudentID=@STUDID";
                        sqlCommand.Parameters.Clear();
                        sqlCommand.Parameters.AddWithValue("STUDID", studentID);
                        sqlCommand.Connection.Open();


                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                StudentClassEnrolment parsedEnrolment = dataReaderToObject(dataReader);
                                if (parsedEnrolment != null)
                                {
                                    if (!returnMe.ContainsKey(studentID))
                                    {
                                        returnMe.Add(studentID, new List<StudentClassEnrolment>());
                                    }
                                    returnMe[studentID].Add(parsedEnrolment);
                                }
                            }
                        }
                        sqlCommand.Connection.Close();
                    }

                }
            }

            return returnMe;
        }

    }
}

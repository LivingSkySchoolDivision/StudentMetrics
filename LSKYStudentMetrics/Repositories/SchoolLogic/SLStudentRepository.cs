using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    public class SLStudentRepository
    {
        private const string SelectSQL = "SELECT Student.iStudentID, Student.dBirthdate, cStudentNumber,GenderLV.cCode as cGender,AborigStatus.cName as cAborigStatus FROM Student " +
                                            "LEFT OUTER JOIN LookupValues as GenderLV ON Student.iLV_GenderID=GenderLV.iLookupValuesID " +
                                            "LEFT OUTER JOIN UserStudent ON Student.iStudentID=UserStudent.iStudentID " +
                                            "LEFT OUTER JOIN LookupValues as AborigStatus ON UserStudent.UF_1656_1=AborigStatus.iLookupValuesID";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, Student> _cache = new Dictionary<int, Student>();

        private Student dataReaderToStudent(SqlDataReader dataReader)
        {
            return new Student()
            {
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                Gender = dataReader["cGender"].ToString().Trim(),
                cStudentNumber = dataReader["cStudentNumber"].ToString().Trim(),
                IsFirstNations = !string.IsNullOrEmpty(dataReader["cAborigStatus"].ToString().Trim()),
                DateOfBirth = Parsers.ParseDate(dataReader["dBirthdate"].ToString().Trim())
            };
        }

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, Student>();
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
                                Student parsedStudent = dataReaderToStudent(dataReader);
                                if (parsedStudent != null)
                                {
                                    _cache.Add(parsedStudent.iStudentID, parsedStudent);
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

        public SLStudentRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _refreshCache();
        }

        public Student Get(int iStudentID)
        {
            if (_cache.ContainsKey(iStudentID))
            {
                return _cache[iStudentID];
            }
            else
            {
                return null;
            }
        }

        public List<Student> GetAll()
        {
            return _cache.Values.ToList();
        }

        public List<int> GetAllIDs()
        {
            return _cache.Keys.ToList();
        }


    }
}

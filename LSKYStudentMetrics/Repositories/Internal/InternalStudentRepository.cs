using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.Internal
{
    public class InternalStudentRepository
    {
        private const string SelectSQL = "SELECT iStudentID, cStudentNumber, lFirstNations,cGender, dDateOfBirth FROM Students";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, Student> _cache = new Dictionary<int, Student>();

        private Student dataReaderToStudent(SqlDataReader dataReader)
        {
            return new Student()
            {
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                Gender = dataReader["cGender"].ToString().Trim(),
                cStudentNumber = dataReader["cStudentNumber"].ToString().Trim(),
                IsFirstNations = Parsers.ParseBool(dataReader["lFirstNations"].ToString().Trim()),
                DateOfBirth = Parsers.ParseDate(dataReader["dDateOfBirth"].ToString().Trim())
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

        public InternalStudentRepository(string SQLConnectionString)
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

        public void Add(List<Student> obj)
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
                        foreach (Student student in obj)
                        {
                            sqlCommand.CommandText = "INSERT INTO Students(iStudentID,cStudentNumber,lFirstNations,dDateOfBirth,cGender) VALUES(@ISTUDENTID,@STUDENTNUM,@FIRSTNATIONS,@DATEOFBIRTH,@GENDER)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("ISTUDENTID", student.iStudentID);
                            sqlCommand.Parameters.AddWithValue("STUDENTNUM", student.cStudentNumber);
                            sqlCommand.Parameters.AddWithValue("FIRSTNATIONS", student.IsFirstNations);
                            sqlCommand.Parameters.AddWithValue("DATEOFBIRTH", student.DateOfBirth);
                            sqlCommand.Parameters.AddWithValue("GENDER", student.Gender);                            
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
        
        public void Update(List<Student> obj)
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
                        foreach (Student student in obj)
                        {                            
                            sqlCommand.CommandText = "UPDATE Students SET cStudentNumber=@STUDENTNUM, lFirstNations=@FIRSTNATIONS, dDateOfBirth=@DATEOFBIRTH, cGender=@GENDER WHERE iStudentID=@ISTUDENTID;";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("ISTUDENTID", student.iStudentID);
                            sqlCommand.Parameters.AddWithValue("STUDENTNUM", student.cStudentNumber);
                            sqlCommand.Parameters.AddWithValue("FIRSTNATIONS", student.IsFirstNations);
                            sqlCommand.Parameters.AddWithValue("DATEOFBIRTH", student.DateOfBirth);
                            sqlCommand.Parameters.AddWithValue("GENDER", student.Gender);                            
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

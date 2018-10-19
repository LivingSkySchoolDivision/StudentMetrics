using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.Internal
{
    public class InternalGradePlacementRepository
    {
        private const string SelectSQL = "SELECT iStudentID, iGradeID, iSchoolYearID FROM StudentGradeLevels";
        private string SQLConnectionString = string.Empty;        

        // dictionary<school year, Dictionary<student id, gradeplacement>>
        private Dictionary<int, Dictionary<int, StudentGradePlacement>> _cacheBySchoolYearID = new Dictionary<int, Dictionary<int, StudentGradePlacement>>();
        
        private Dictionary<int, List<StudentGradePlacement>> _cacheByStudentID = new Dictionary<int, List<StudentGradePlacement>>();
        
        private InternalGradeLevelRepository _gradeRepo;
        private InternalSchoolYearRepository _schoolYearRepo;

        private StudentGradePlacement dataReaderToObject(SqlDataReader dataReader)
        {
            return new StudentGradePlacement()
            {
                iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                iGradeID = Parsers.ParseInt(dataReader["iGradeID"].ToString().Trim()),
                iSchoolYearID = Parsers.ParseInt(dataReader["iSchoolYearID"].ToString().Trim()),
                GradeLevel = _gradeRepo.Get(Parsers.ParseInt(dataReader["iGradeID"].ToString().Trim())),
                SchoolYear = _schoolYearRepo.Get(Parsers.ParseInt(dataReader["iSchoolYearID"].ToString().Trim()))
            };
        }

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cacheBySchoolYearID = new Dictionary<int, Dictionary<int, StudentGradePlacement>>();
                _cacheByStudentID = new Dictionary<int, List<StudentGradePlacement>>();
               
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
                                StudentGradePlacement parsedObject = dataReaderToObject(dataReader);
                                if (parsedObject != null)
                                {
                                    if ((parsedObject.GradeLevel != null) && (parsedObject.SchoolYear != null))
                                    {
                                        // If this school year Id is new, set it up in the dictionary
                                        if (!_cacheBySchoolYearID.ContainsKey(parsedObject.iSchoolYearID))
                                        {
                                            _cacheBySchoolYearID.Add(parsedObject.iSchoolYearID, new Dictionary<int, StudentGradePlacement>());
                                        }
                                                                                
                                        if (!_cacheBySchoolYearID[parsedObject.iSchoolYearID].ContainsKey(parsedObject.iStudentID))
                                        {
                                            _cacheBySchoolYearID[parsedObject.iSchoolYearID].Add(parsedObject.iStudentID, parsedObject);
                                        }

                                        if (!_cacheByStudentID.ContainsKey(parsedObject.iStudentID))
                                        {
                                            _cacheByStudentID.Add(parsedObject.iStudentID, new List<StudentGradePlacement>());
                                        }
                                        _cacheByStudentID[parsedObject.iStudentID].Add(parsedObject);
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
        }

        public StudentGradePlacement Get(SchoolYear year, Student student)
        {
            return Get(year.ID, student.iStudentID);
        }

        public StudentGradePlacement Get(SchoolYear year, int iStudentID)
        {
            return Get(year.ID, iStudentID);
        }

        public StudentGradePlacement Get(int iSchoolYearID, int iStudentID)
        {
            if (_cacheBySchoolYearID.ContainsKey(iSchoolYearID))
            {
                if (_cacheBySchoolYearID[iSchoolYearID].ContainsKey(iStudentID))
                {
                    return _cacheBySchoolYearID[iSchoolYearID][iStudentID];
                }
            }
            return null;
        }

        public List<StudentGradePlacement> GetAllForSchoolYear(int iSchoolYearID)
        {
            if (_cacheBySchoolYearID.ContainsKey(iSchoolYearID))
            {
                return _cacheBySchoolYearID[iSchoolYearID].Values.ToList<StudentGradePlacement>();
            }
            return new List<StudentGradePlacement>();
        }

        public List<StudentGradePlacement> GetAllForStudent(int iStudentID)
        {
            if (_cacheByStudentID.ContainsKey(iStudentID))
            {
                return _cacheByStudentID[iStudentID];
            }
            return new List<StudentGradePlacement>();
        }

        public InternalGradePlacementRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            this._gradeRepo = new InternalGradeLevelRepository(SQLConnectionString);
            this._schoolYearRepo = new InternalSchoolYearRepository(SQLConnectionString);
            _refreshCache();
        }            

        public void Add(List<StudentGradePlacement> objs)
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
                        foreach (StudentGradePlacement obj in objs)
                        {
                            sqlCommand.CommandText = "INSERT INTO StudentGradeLevels(iStudentID, iGradeID, iSchoolYearID) VALUES(@STUDID,@GRADELEVELID,@SCHOOLYRID)";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STUDID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("GRADELEVELID", obj.iGradeID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLYRID", obj.iSchoolYearID);
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

        public void Update(List<StudentGradePlacement> objs)
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
                        foreach (StudentGradePlacement obj in objs)
                        {
                            sqlCommand.CommandText = "UPDATE StudentGradeLevels SET iGradeID=@GRADELEVELID WHERE iStudentID=@STUDID AND iSchoolYearID=@SCHOOLYRID";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.AddWithValue("STUDID", obj.iStudentID);
                            sqlCommand.Parameters.AddWithValue("GRADELEVELID", obj.iGradeID);
                            sqlCommand.Parameters.AddWithValue("SCHOOLYRID", obj.iSchoolYearID);
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

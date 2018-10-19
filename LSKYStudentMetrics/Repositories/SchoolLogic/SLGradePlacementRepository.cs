using LSKYStudentMetrics.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    // This repository will only ever work for a single school year, unlike the internal one
    // so, it's structure is going to be different from it's internal counterpart

    public class SLGradePlacementRepository
    {
        private const string SelectSQL = "SELECT student.iStudentID, Student.iGradesID FROM student WHERE iGradesID>0";
        private string SQLConnectionString = string.Empty;        
        private SchoolYear schoolYear = null;
        private Dictionary<int, StudentGradePlacement> _cache = new Dictionary<int, StudentGradePlacement>();

        private SLGradeLevelRepository _gradeRepo;        

        private StudentGradePlacement dataReaderToObject(SqlDataReader dataReader)
        {
            GradeLevel gradeLevel = _gradeRepo.Get(Parsers.ParseInt(dataReader["iGradesID"].ToString().Trim()));
            // Filter out invalid grades here, to save processing time when filtering out later
            if (gradeLevel != null) {
                return new StudentGradePlacement()
                {
                    iStudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                    iGradeID = Parsers.ParseInt(dataReader["iGradesID"].ToString().Trim()),
                    iSchoolYearID = this.schoolYear.ID,
                    GradeLevel = gradeLevel,
                    SchoolYear = this.schoolYear
                };
            }
            return null;
        }

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, StudentGradePlacement>();
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
                                    // Ignore any students with iGradesID of 0
                                    if (parsedObject.iGradeID > 0)
                                    {
                                        _cache.Add(parsedObject.iStudentID, parsedObject);
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

        public SLGradePlacementRepository(string SQLConnectionString, SchoolYear schoolYear)
        {
            this.SQLConnectionString = SQLConnectionString;
            this.schoolYear = schoolYear;
            if (this.schoolYear == null)
            {
                throw new InvalidSchoolYearException("Invalid school year");
            }
            if (this.schoolYear.ID <= 0)
            {
                throw new InvalidSchoolYearException("Invalid school year");
            }

            this._gradeRepo = new SLGradeLevelRepository(SQLConnectionString);           

            _refreshCache();
        }

        public List<int> GetAllIDs()
        {
            return _cache.Keys.ToList();
        }

        public List<StudentGradePlacement> GetAll()
        {
            return _cache.Values.ToList();
        }


    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Repositories.SchoolLogic
{
    class SLClassScheduleRepository
    {
        private SLClassTermRepository _classTermRepository;

        private string _SQLConnectionString = string.Empty;
        private Dictionary<int, SchoolClassSchedule> _cache;
        private const string selectSQL = "SELECT iClassScheduleID,iTermID,iBlockNumber,iDayNumber,ClassResource.iClassID FROM ClassSchedule LEFT OUTER JOIN ClassResource ON ClassSChedule.iClassResourceID=ClassResource.iClassResourceID WHERE ClassResource.iClassID IS NOT NULL";
        
        public SLClassScheduleRepository(string SQLConnectionString)
        {
            this._SQLConnectionString = SQLConnectionString;
            _classTermRepository = new SLClassTermRepository(SQLConnectionString);

            _cache = new Dictionary<int, SchoolClassSchedule>();

            using (SqlConnection connection = new SqlConnection(SQLConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = connection;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = selectSQL;
                    sqlCommand.Connection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int termID = Parsers.ParseInt(dataReader["iTermID"].ToString().Trim());
                            int blockNum = Parsers.ParseInt(dataReader["iBlockNumber"].ToString().Trim());
                            int dayNum = Parsers.ParseInt(dataReader["iDayNumber"].ToString().Trim());
                            int classID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim());

                            if ((termID > 0) && (blockNum > 0) && (dayNum > 0) && (classID > 0))
                            {
                                List<Term> classTerms = _classTermRepository.Get(classID);

                                if (!_cache.ContainsKey(classID))
                                {
                                    _cache.Add(classID, new SchoolClassSchedule(classID, classTerms));
                                }
                                _cache[classID].AddScheduleDay(termID, dayNum, blockNum);
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }
        }

        public SchoolClassSchedule GetForClass(int iClassID)
        {
            if (_cache.ContainsKey(iClassID))
            {
                return _cache[iClassID];
            } else
            {
                return new SchoolClassSchedule(iClassID);
            }
        }
    }
}

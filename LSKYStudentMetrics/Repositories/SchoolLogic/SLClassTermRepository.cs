using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    class SLClassTermRepository
    {
        private SLTermRepository _termRepo;
        private Dictionary<int, List<Term>> _termsByClass;
        private string _SQLConnectionString = string.Empty;

        private const string selectSQL = "SELECT iClassID, iTermID FROM ClassTerm";
        
        public SLClassTermRepository(string SQLConnectionString)
        {
            this._termRepo = new SLTermRepository(SQLConnectionString);
            this._SQLConnectionString = SQLConnectionString;

            this._termsByClass = new Dictionary<int, List<Term>>();

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
                            int iClassID = Parsers.ParseInt(dataReader["iClassID"].ToString().Trim());
                            int iTermID = Parsers.ParseInt(dataReader["iTermID"].ToString().Trim());
                            if ((iClassID > 0) && (iTermID > 0))
                            {
                                Term t = _termRepo.Get(iTermID);
                                if (t != null)
                                {
                                    if (!_termsByClass.ContainsKey(iClassID))
                                    {
                                        _termsByClass.Add(iClassID, new List<Term>());
                                    }
                                    _termsByClass[iClassID].Add(t);
                                }
                            }
                        }
                    }
                    sqlCommand.Connection.Close();
                }
            }

        }

        public List<Term> Get(int iTermID)
        {
            if (_termsByClass.ContainsKey(iTermID))
            {
                return _termsByClass[iTermID];
            } else
            {
                return new List<Term>();
            }
        }

    }
}

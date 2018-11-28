﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Repositories.SchoolLogic
{
    public class SLAbsenceReasonRepository
    {
        private const string SelectSQL = "SELECT iAttendanceReasonsID, cName, lExcusable FROM AttendanceReasons";
        private string SQLConnectionString = string.Empty;
        private Dictionary<int, AbsenceReason> _cache = new Dictionary<int, AbsenceReason>();

        private AbsenceReason dataReaderToObject(SqlDataReader dataReader)
        {
            return new AbsenceReason()
            {
                ID = Parsers.ParseInt(dataReader["iAttendanceReasonsID"].ToString().Trim()),
                Content = dataReader["cName"].ToString().Trim(),
                IsExcusable = Parsers.ParseBool(dataReader["lExcusable"].ToString().Trim())
            };
        }

        private void _refreshCache()
        {
            if (!string.IsNullOrEmpty(this.SQLConnectionString))
            {
                _cache = new Dictionary<int, AbsenceReason>();
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
                                AbsenceReason parsedObject = dataReaderToObject(dataReader);
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

        public SLAbsenceReasonRepository(string SQLConnectionString)
        {
            this.SQLConnectionString = SQLConnectionString;
            _refreshCache();
        }

        public List<int> GetAllIDs()
        {
            return _cache.Keys.ToList();
        }

        public AbsenceReason Get(int iAbsenceStatusID)
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

        public List<AbsenceReason> GetAll()
        {
            return _cache.Values.ToList();
        }
    }
}

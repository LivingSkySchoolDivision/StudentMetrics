using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDatabaseSafeDate(this DateTime obj)
        {
            if (obj < new DateTime(1753,01,01))
            {
                return new DateTime(1753, 01, 01);
            }

            if (obj > new DateTime(9999,12,31))
            {
                return new DateTime(9999, 12, 31);
            }

            return obj;
        }
                
        /// <summary>
        /// Does this date represent a "null" date, as imported from a SQL database, where the datetime field was not set up as nullable?    
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsDatabaseNullDate(this DateTime obj)
        {
            // SchoolLogic uses 1900-01-01 as a null date
            if (obj.Year < 1901)
            {
                return true;
            }

            return false;
        }
    }
}

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
    }
}

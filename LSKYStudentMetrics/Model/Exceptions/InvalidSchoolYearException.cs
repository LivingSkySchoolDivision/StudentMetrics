using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class InvalidSchoolYearException : Exception
    {
        public InvalidSchoolYearException() { }

        public InvalidSchoolYearException(string message) : base(message) { }

        public InvalidSchoolYearException(string message, Exception inner) : base(message, inner) { }
    }
}

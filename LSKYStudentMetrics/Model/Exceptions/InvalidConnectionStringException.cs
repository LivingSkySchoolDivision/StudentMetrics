using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    class InvalidConnectionStringException : Exception
    {
        public InvalidConnectionStringException() { }

        public InvalidConnectionStringException(string message) : base(message) { }

        public InvalidConnectionStringException(string message, Exception inner) : base(message, inner) { }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    class NoGraphDataException : Exception
    {
        public NoGraphDataException() { }

        public NoGraphDataException(string message) : base(message) { }

        public NoGraphDataException(string message, Exception inner) : base(message, inner) { }
    }
}

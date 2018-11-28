using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class InvalidConfigFileException : Exception
    {
        public InvalidConfigFileException() { }

        public InvalidConfigFileException(string message) : base(message) { }

        public InvalidConfigFileException(string message, Exception inner) : base(message, inner) { }
    }
}

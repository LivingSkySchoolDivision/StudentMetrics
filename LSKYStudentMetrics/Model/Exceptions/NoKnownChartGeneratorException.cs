using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    class NoKnownChartGeneratorException : Exception
    {
        public NoKnownChartGeneratorException() { }

        public NoKnownChartGeneratorException(string message) : base(message) { }

        public NoKnownChartGeneratorException(string message, Exception inner) : base(message, inner) { }
    }
}

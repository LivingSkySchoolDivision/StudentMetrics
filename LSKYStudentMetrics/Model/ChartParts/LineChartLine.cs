using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Model.ChartParts
{
    class LineChartLine
    {
        public string Label { get; set; }
        public Dictionary<string, decimal> LineDataPoints { get; set; }

        public LineChartLine()
        {
            this.LineDataPoints = new Dictionary<string, decimal>();
        }
    }
}

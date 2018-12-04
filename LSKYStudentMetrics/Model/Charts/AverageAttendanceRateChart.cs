using LSSDMetricsLibrary.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class AverageAttendanceRateChart : iChart
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public byte[] Generate(string SQLConnectionString)
        {
            AverageAttendanceRateChartGenerator generator = new AverageAttendanceRateChartGenerator(SQLConnectionString, this);
            return generator.DrawGraph();
        }
    }
}

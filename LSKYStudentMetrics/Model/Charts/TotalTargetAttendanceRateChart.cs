using LSSDMetricsLibrary.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class TotalTargetAttendanceRateChart : iChart
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TargetRate { get; set; }

        public byte[] Generate(string SQLConnectionString)
        {
            TotalTargetAttendanceRateChartGenerator generator = new TotalTargetAttendanceRateChartGenerator(SQLConnectionString, this);
            return generator.DrawGraph();
        }
    }
}

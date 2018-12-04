using LSSDMetricsLibrary.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class FNMTargetAttendanceRateChart : iChart
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TargetRate { get; set; }

        public byte[] Generate(string SQLConnectionString)
        {
            FNMTargetAttendanceRateChartGenerator generator = new FNMTargetAttendanceRateChartGenerator(SQLConnectionString, this);
            return generator.DrawGraph();
        }
    }
}

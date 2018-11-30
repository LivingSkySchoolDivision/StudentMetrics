using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Charts
{
    public class BarChartDataSeries
    {
        public string Label { get; set; }
        public List<BarChartPercentBar> DataPoints { get; set; }

        public BarChartDataSeries()
        {
            this.DataPoints = new List<BarChartPercentBar>();
        }
    }
}

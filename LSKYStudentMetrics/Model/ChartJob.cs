using LSSDMetricsLibrary.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    /// <summary>
    /// Represents a job to create a chart. Contains all of the options that any chart may need - it's up to the chart generator method to take what it needs from this object.
    /// </summary>
    public class ChartJob
    {
        public string JobName { get; set; }
        public ChartType ChartType { get; set; }
        private DateTime _givenStartDate;
        private DateTime _givenEndDate;
        public DateTime StartDate {
            get
            {
                if (_givenStartDate > DateTime.Today)
                {
                    return DateTime.Today;
                }
                return _givenStartDate;
            }
            set
            {
                _givenStartDate = value;
            }
        }

        public DateTime EndDate
        {
            get
            {
                if (_givenEndDate > DateTime.Today)
                {
                    return DateTime.Today;
                }
                return _givenEndDate;
            }
            set
            {
                _givenEndDate = value;
            }
        }
        public decimal TargetAttendanceRate { get; set; }

        public override string ToString()
        {
            return "{ChartJob Type:" + this.ChartType + ", From: " + this.StartDate + ", To:" + this.EndDate + "}";
        }

        public byte[] Generate(string SQLConnectionString)
        {
            // Determine what kind of chart this is, and pass this object along to the appropriate generator

            switch(this.ChartType)
            {
                case ChartType.AverageAttendanceRate:
                    return (new AverageAttendanceRateChartGenerator(SQLConnectionString, this).DrawGraph());
                case ChartType.FNMTargetAttendanceRate:
                    return (new FNMTargetAttendanceRateChartGenerator(SQLConnectionString, this).DrawGraph());
                case ChartType.TotalTargetAttendanceRate:
                    return (new TotalTargetAttendanceRateChartGenerator(SQLConnectionString, this).DrawGraph());
            }          

            throw new NoKnownChartGeneratorException("Unknown chart type.");
        }
    }
}

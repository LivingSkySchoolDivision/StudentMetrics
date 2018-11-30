using LSSDMetricsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Charts
{
    public class AverageAttendanceRateGraphDataPoint : IComparable<AverageAttendanceRateGraphDataPoint>
    {
        public School School { get; set; }
        public decimal AttendanceRate { get; set; }

        public decimal AttendanceRate_FNM { get; set; }

        public string FriendlyAttendanceRate
        {
            get
            {
                return (this.AttendanceRate * 100).ToString("0.##") + "%";
            }
        }

        public string FriendlyAttendanceRate_FNM
        {
            get
            {
                return (this.AttendanceRate_FNM * 100).ToString("0.##") + "%";
            }
        }

        public int CompareTo(AverageAttendanceRateGraphDataPoint that)
        {
            if (this.AttendanceRate > that.AttendanceRate) { return -1; }
            if (this.AttendanceRate == that.AttendanceRate) { return 0; }
            return 1;
        }

        public override string ToString()
        {
            return this.FriendlyAttendanceRate;
        }
    }
}

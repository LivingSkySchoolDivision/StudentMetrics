using LSKYStudentMetrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.GraphDataPoints
{
    class AverageAttendanceRateGraphDataPoint : IComparable<AverageAttendanceRateGraphDataPoint>
    {
        public School School { get; set; }
        public float AttendanceRate { get; set; }

        public float AttendanceRate_FNM { get; set; }

        public string FriendlyAttendanceRate
        {
            get
            {
                return (this.AttendanceRate * 100) + "%";
            }
        }

        public string FriendlyAttendanceRate_FNM
        {
            get
            {
                return (this.AttendanceRate_FNM * 100) + "%";
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

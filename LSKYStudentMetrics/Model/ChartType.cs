using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    /*
     * This enum represents the types of charts that can be saved as jobs and deserialized.
     */

    /// <summary>
    /// Type of chart, for serialization purposes.
    /// </summary>
    public enum ChartType
    {
        UNKNOWN,
        AverageAttendanceRate,
        TotalTargetAttendanceRate,
        FNMTargetAttendanceRate
    }
}

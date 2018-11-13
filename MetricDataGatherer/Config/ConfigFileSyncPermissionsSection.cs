using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer
{
    public class ConfigFileSyncPermissionsSection
    {
        public bool AllowSync { get; set; }
        public bool AllowAdds { get; set; }
        public bool AllowUpdates { get; set; }
        public bool AllowRemovals { get; set; }
        public bool ForceUpdate { get; set; }

        private static string toX(bool b)
        {
            return b ? "X" : " ";
        }

        public override string ToString()
        {
            return "{ SYNC[" + toX(this.AllowSync) + "] ADD[" + toX(this.AllowAdds) + "] UPDATE[" + toX(this.AllowUpdates) + "] REMOVE[" + toX(this.AllowRemovals) + "] FORCE[" + toX(this.ForceUpdate) + "] }";
        }
    }
}

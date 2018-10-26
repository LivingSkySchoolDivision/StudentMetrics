using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.Config
{
    public class ConfigFileSyncPermissionsSection
    {
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
            return "{ A[" + toX(this.AllowAdds) + "] U[" + toX(this.AllowUpdates) + "] R[" + toX(this.AllowRemovals) + "] F[" + toX(this.ForceUpdate) + "] }";
        }
    }
}

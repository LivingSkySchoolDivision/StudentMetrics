using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.SyncEngine
{
    class StudentGradePlacementSync
    {
        public delegate void LogDelegate(string msg);
        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
        }
    }
}

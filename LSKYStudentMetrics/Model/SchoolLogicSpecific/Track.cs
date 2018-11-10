using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    // Make me private
    public class Track
    {
        public int iTrackID { get; set; }
        public bool isDaily { get; set; }
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }
        public bool isPeriod { get { return !isDaily; } }
        public TrackSchedule Schedule { get; set; }
        public int DailyBlocksPerDay { get; set; }
        
    }
}

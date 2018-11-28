using LSSDMetricsLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class SchoolClassSchedule
    {
        public int iClassID { get; set; }

        private List<Term> _classTerms = new List<Term>();

        // Dictionary<term ID, Dictionary<day number, List<block number>>>
        private Dictionary<int, Dictionary<int, List<int>>> _classBlockNumbersByDay = new Dictionary<int, Dictionary<int, List<int>>>();
        

        public SchoolClassSchedule(int iClassID)
        {
            this.iClassID = iClassID;
            this._classTerms = new List<Term>();
            this._classBlockNumbersByDay = new Dictionary<int, Dictionary<int, List<int>>>();
        }

        public SchoolClassSchedule(int iClassID, List<Term> classTerms)
        {
            this._classTerms = classTerms;
            this.iClassID = iClassID;
            this._classBlockNumbersByDay = new Dictionary<int, Dictionary<int, List<int>>>();
        }

        /// <summary>
        /// Returns the block number that this class is scheduled for on the given date, or zero if the class is not scheduled on that day
        /// </summary>
        /// <param name="thisDate"></param>
        /// <returns></returns>
        public List<int> GetBlocksScheduledOn(DateTime thisDate)
        {
            // Check to see if the given date falls within any of this class's terms 
            //  - Make a list of any terms that it falls in
            // If the list of terms is > 0, continue
            // Get the school day number of the given date

            foreach(Term term in _classTerms)
            {
                if ((thisDate >= term.Starts) && (thisDate <= term.Ends))
                {
                    // Get the day number for this day
                    int dayNumber = term.Track.Schedule.GetSchoolDayOn(thisDate);
                    if (dayNumber > 0)
                    {
                        if (_classBlockNumbersByDay.ContainsKey(term.ID))
                        {
                            if (_classBlockNumbersByDay[term.ID].ContainsKey(dayNumber))
                            {
                                return _classBlockNumbersByDay[term.ID][dayNumber];
                            }
                        }                        
                    }
                }
            }

            return new List<int>();
        }
                
        public void AddScheduleDay(int term, int day, int block)
        {
            if ((term > 0) && (day > 0) && (block > 0))
            {
                if (!_classBlockNumbersByDay.ContainsKey(term))
                {
                    _classBlockNumbersByDay.Add(term, new Dictionary<int, List<int>>());
                }

                if (!_classBlockNumbersByDay[term].ContainsKey(day))
                {
                    _classBlockNumbersByDay[term].Add(day, new List<int>());
                }

                if (!_classBlockNumbersByDay[term][day].Contains(block))
                {
                    _classBlockNumbersByDay[term][day].Add(block);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class SchoolClassSchedule
    {
        private List<Term> _classTerms = new List<Term>();

        // Dictionary<term ID, Dictionary<day number, List<block number>>>
        private Dictionary<int, Dictionary<int, List<int>>> _classBlockNumbersByDay = new Dictionary<int, Dictionary<int, List<int>>>();
        

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
                        if (_classBlockNumbersByDay[term.ID].ContainsKey(dayNumber))
                        {
                            return _classBlockNumbersByDay[term.ID][dayNumber];
                        }
                    }
                }
            }

            return new List<int>();
        }
                
        public void AddScheduleDay(int term, int day, int block)
        {
            // THIS WAS COPIED OVER FROM THE TRACK SCHEDULE AND WILL NEED TO BE REWRITTEN
            if (!string.IsNullOrEmpty(dayNumber))
            {
                if ((dayNumber != "N") && (dayNumber != "0"))
                {
                    int parsedDayNum = Parsers.ParseInt(dayNumber);
                    if (parsedDayNum > 0)
                    {
                        if (!_days.ContainsKey(date.Year))
                        {
                            _days.Add(date.Year, new Dictionary<int, Dictionary<int, int>>());
                        }

                        if (!_days[date.Year].ContainsKey(date.Month))
                        {
                            _days[date.Year].Add(date.Month, new Dictionary<int, int>());
                        }

                        if (!_days[date.Year][date.Month].ContainsKey(date.Day))
                        {
                            _days[date.Year][date.Month].Add(date.Day, parsedDayNum);
                        }
                        else
                        {
                            _days[date.Year][date.Month][date.Day] = parsedDayNum;
                        }
                    }
                }
            }
        }



    }
}

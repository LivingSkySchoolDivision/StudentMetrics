using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class TrackSchedule
    {
        // Dictionary<year, Dictionary<month,Dictionary<day,schooldaynumber>>>
        private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _days = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();

        public bool IsInstructional(DateTime thisDate)
        {
            return (GetSchoolDayOn(thisDate) > 0);
        }

        public int GetSchoolDayOn(DateTime thisDate)
        {
            if (_days.ContainsKey(thisDate.Year))
            {
                if (_days[thisDate.Year].ContainsKey(thisDate.Month))
                {
                    if (_days[thisDate.Year][thisDate.Month].ContainsKey(thisDate.Day))
                    {
                        return _days[thisDate.Year][thisDate.Month][thisDate.Day];
                    }
                }
            }

            return 0;
        }

        public void AddScheduleDay(DateTime date, string dayNumber)
        {
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class CalendarDay
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }


        public static List<CalendarDay> GetCalendarDaysBetween(DateTime start, DateTime end)
        {
            return GetCalendarDaysBetween(start, end, false);
        }

        public static List<CalendarDay> GetCalendarDaysBetween(DateTime start, DateTime end, bool skipWeekends)
        {
            List<CalendarDay> returnMe = new List<CalendarDay>();

            // First make sure that the start date happens before the end date
            DateTime actualStartDate = start;
            DateTime actualEndDate = end;
            if (start > end)
            {
                actualStartDate = end;
                actualEndDate = start;
            }

            for (DateTime day = actualStartDate.Date; day.Date <= actualEndDate.Date; day = day.AddDays(1))
            {
                if (skipWeekends) { if ((day.DayOfWeek == DayOfWeek.Saturday) || (day.DayOfWeek == DayOfWeek.Sunday)) { continue; } }
                returnMe.Add(new CalendarDay() { Year = day.Year, Month = day.Month, Day = day.Day });
            }

            return returnMe;
        }

        public static implicit operator DateTime(CalendarDay day)
        {
            return new DateTime(day.Year, day.Month, day.Day);
        } 

        public static implicit operator CalendarDay(DateTime day)
        {
           return new CalendarDay() { Year = day.Year, Month = day.Month, Day = day.Day };
        }

        public override string ToString()
        {
            // Yes, I realize this is bad because it doesn't account for different date formats
            // but at the moment this is probably only going to be used for debugging purposes
            return this.Year + "/" + this.Month + "/" + this.Day;
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public static class Parsers
    {
        /// <summary>
        /// A potentially easier to use wrapper for the parse int function
        /// </summary>
        /// <param name="thisString"></param>
        /// <returns></returns>
        public static int ParseInt(string thisString)
        {
            int returnMe = 0;

            if (Int32.TryParse(thisString, out returnMe))
            {
                return returnMe;
            }

            return 0;
        }

        public static char ParseChar(string thisString)
        {
            char returnMe = new char();
            return Char.TryParse(thisString, out returnMe) ? returnMe : new char();
        }

        public static float ParseFloat(string thisString)
        {
            float returnMe = 0;
            return Single.TryParse(thisString, out returnMe) ? returnMe : 0;
        }

        public static decimal ParseDecimal(string thisString)
        {
            decimal returnMe = -1;
            if (Decimal.TryParse(thisString, out returnMe))
            {
                return returnMe;
            }
            return 0;
        }
             

        public static DateTime ParseDate(string thisDate)
        {
            DateTime returnMe = DateTime.MinValue;

            if (!DateTime.TryParse(thisDate, out returnMe))
            {
                returnMe = DateTime.MinValue;
            }          

            return returnMe;
        }

        /// <summary>
        /// Parses a date, specifically for parsing custom date pickers
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime ParseDate(string year, string month, string day)
        {
            int parsedYear = DateTime.Now.Year;
            int parsedMonth = DateTime.Now.Month;
            int parsedDay = DateTime.Now.Day;

            if (!Int32.TryParse(year, out parsedYear))
            {
                return DateTime.MinValue;
            }

            if (!Int32.TryParse(month, out parsedMonth))
            {
                return DateTime.MinValue;
            }

            if (!Int32.TryParse(day, out parsedDay))
            {
                return DateTime.MinValue;
            }

            if (parsedDay > DateTime.DaysInMonth(parsedYear, parsedMonth))
                parsedDay = DateTime.DaysInMonth(parsedYear, parsedMonth);

            return new DateTime(parsedYear, parsedMonth, parsedDay);
        }

        /// <summary>
        /// Parse a bool from a database or other text source
        /// </summary>
        /// <param name="thisDatabaseValue"></param>
        /// <returns></returns>
        public static bool ParseBool(string thisDatabaseValue)
        {
            if (String.IsNullOrEmpty(thisDatabaseValue))
            {
                return false;
            }
            else
            {
                if (thisDatabaseValue.ToLower().Equals("yes")) { return true; }
                if (thisDatabaseValue.ToLower().Equals("no")) { return false; }
                if (thisDatabaseValue.ToLower().Equals("true")) { return true; }
                if (thisDatabaseValue.ToLower().Equals("false")) { return false; }

                bool parsedBool = false;
                Boolean.TryParse(thisDatabaseValue, out parsedBool);
                return parsedBool;
            }
        }

        /// <summary>
        /// SchoolLogic puts a leading zero in front of the grade for some reason. This removes it and formats it properly
        /// </summary>
        /// <param name="unformattedGrade"></param>
        /// <returns></returns>
        public static string FormatGrade(string unformattedGrade)
        {
            string returnMe;

            if (unformattedGrade.ToLower() == "0k")
            {
                returnMe = "K";
            }
            else if (unformattedGrade.ToLower() == "k")
            {
                returnMe = "K";
            }
            else if (unformattedGrade.ToLower() == "pk")
            {
                returnMe = "PK";
            }
            else
            {
                returnMe = Parsers.ParseInt(unformattedGrade).ToString();
            }

            return returnMe;

        }

        public static List<int> ParseInt(List<string> numbers)
        {
            return numbers.Select(Parsers.ParseInt).ToList();
        }

        public static IEnumerable<DateTime> GetEachDayBetween(DateTime dateFrom, DateTime dateTo)
        {
            // Just get the date, delete the time
            DateTime from = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0);
            DateTime to = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 0, 0, 0);

            // Dates need to be in chronological order, so reverse them if necesary
            if (dateFrom > dateTo)
            {
                to = dateFrom;
                @from = dateTo;
            }

            List<DateTime> returnMe = new List<DateTime>();
            for (DateTime day = @from.Date; day.Date <= to.Date; day = day.AddDays(1))
            {
                returnMe.Add(day);
            }
            return returnMe;
        }

        /// <summary>
        /// Finds the school year of the given date, returning the year that the school year STARTS in
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int FindSchoolYear(DateTime date)
        {
            if ((date.Month >= 1) && (date.Month <= 7))
            {
                return date.Year - 1;
            }
            else
            {
                return date.Year;
            }
        }

    }
}

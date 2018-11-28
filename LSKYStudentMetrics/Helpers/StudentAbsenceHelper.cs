using LSSDMetricsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class StudentAbsenceHelper
    {
        public int iStudentID { get; set; }
        Dictionary<int, Dictionary<int, Dictionary<int, List<Absence>>>> _allAbsences = new Dictionary<int, Dictionary<int, Dictionary<int, List<Absence>>>>();

        public StudentAbsenceHelper(int iStudentID)
        {
            this.iStudentID = iStudentID;
        }

        public StudentAbsenceHelper(int iStudentID, List<Absence> Absences)
        {
            this.iStudentID = iStudentID;
            this.AddAbsences(Absences);
        }

        public List<Absence> AllAbsencesOn(CalendarDay thisDay)
        {
            return AllAbsencesOn(thisDay.Year, thisDay.Month, thisDay.Day);
        }

        public List<Absence> AllAbsencesOn(DateTime thisDate)
        {
            return AllAbsencesOn(thisDate.Year, thisDate.Month, thisDate.Day);
        }

        public List<Absence> AllAbsencesOn(int year, int month, int day)
        {
            if (_allAbsences.ContainsKey(year))
            {
                if (_allAbsences[year].ContainsKey(month))
                {
                    if (_allAbsences[year][month].ContainsKey(day))
                    {
                        return _allAbsences[year][month][day];
                    }
                }
            }

            return new List<Absence>();
        }

        public int NegativeAttendanceOn(CalendarDay thisDay)
        {
            return NegativeAttendanceOn(thisDay.Year, thisDay.Month, thisDay.Day);
        }

        public int NegativeAttendanceOn(DateTime thisDate)
        {
            return NegativeAttendanceOn(thisDate.Year, thisDate.Month, thisDate.Day);
        }

        public int NegativeAttendanceOn(int year, int month, int day)
        {
            if (_allAbsences.ContainsKey(year))
            {
                if (_allAbsences[year].ContainsKey(month))
                {
                    if (_allAbsences[year][month].ContainsKey(day))
                    {
                        return _allAbsences[year][month][day].Count(x => x.CountsAgainstAttendanceRate);
                    }
                }
            }

            return 0;
        }

        // Make an Add() method to convert and sort absences
        public void AddAbsence(Absence abs)
        {
            if (abs != null)
            {
                if (!_allAbsences.ContainsKey(abs.Date.Year))
                {
                    _allAbsences.Add(abs.Date.Year, new Dictionary<int, Dictionary<int, List<Absence>>>());
                }

                if (!_allAbsences[abs.Date.Year].ContainsKey(abs.Date.Month))
                {
                    _allAbsences[abs.Date.Year].Add(abs.Date.Month, new Dictionary<int, List<Absence>>());
                }

                if (!_allAbsences[abs.Date.Year][abs.Date.Month].ContainsKey(abs.Date.Day))
                {
                    _allAbsences[abs.Date.Year][abs.Date.Month].Add(abs.Date.Day, new List<Absence>());
                }

                _allAbsences[abs.Date.Year][abs.Date.Month][abs.Date.Day].Add(abs);
            }            
        }

        public void AddAbsences(List<Absence> abs)
        {
            foreach(Absence a in abs)
            {
                AddAbsence(a);
            }
        }

    }


    
}

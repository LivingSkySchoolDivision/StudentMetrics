﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class Student
    {
        /* **************************************************************************** */
        /* * DONT ADD HELPERS LIKE StudentSchedule or StudentExpectedAttendance here  * */
        /* *                                                                          * */
        /* * Those helpers are "side specific", and don't make sense to both sides    * */
        /* * of the sync. Use a dictionary instead.                                   * */        
        /* **************************************************************************** */

        public int iStudentID { get; set; }
        public string cStudentNumber { get; set; }

        public bool IsFirstNations { get; set; }

        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        public List<StudentGradePlacement> GradePlacements { get; set; }

        public GradeLevel GradeOn(DateTime thisDate)
        {
            foreach (StudentGradePlacement gp in GradePlacements)
            {
                if ((gp.SchoolYear.Starts <= thisDate) && (gp.SchoolYear.Ends >= thisDate))
                {
                    return gp.GradeLevel;
                }
            }

            return GradeLevel.Unknown;
        }

        public Student()
        {
            this.GradePlacements = new List<StudentGradePlacement>();
        }


        public UpdateCheck CheckIfUpdatesAreRequired(Student student)
        {
            // Check to make sure that the ID matches, and return -1 if it does not
            if (this.iStudentID != student.iStudentID)
            {
                return UpdateCheck.NotSameObject;
            }

            // Check all properties of the objects to see if they are different
            int updates = 0;

            if (!this.cStudentNumber.Equals(student.cStudentNumber)) { updates++; }
            if (!this.IsFirstNations.Equals(student.IsFirstNations)) { updates++; }
            if (!this.Gender.Equals(student.Gender)) { updates++; }
            if (!this.DateOfBirth.Equals(student.DateOfBirth)) { updates++; }

            if (updates == 0)
            {
                return UpdateCheck.NoUpdateRequired;
            }
            else
            {
                return UpdateCheck.UpdatesRequired;
            }
        }

        public override string ToString()
        {
            return this.iStudentID + " (" + this.cStudentNumber + ")";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class Student
    {
        public int iStudentID { get; set; }
        public string cStudentNumber { get; set; }

        public bool IsFirstNations { get; set; }

        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        public List<StudentGradePlacement> GradePlacements { get; set; }


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
    }
}

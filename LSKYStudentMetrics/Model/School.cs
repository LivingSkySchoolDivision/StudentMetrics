using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary
{
    public class School
    {
        public int iSchoolID { get; set; }
        public string GovernmentID { get; set; }
        public string Name { get; set; }

        public string ShortName { get
            {
                return this.Name.Replace("High", "").Replace("School","").Replace("Elementary","");
                
            }
        }

        /// <summary>
        /// Checks the "Old" object (this object) against a potentially new object (the object passed as a parameter) to see if the old one should be updated
        /// </summary>
        /// <param name="school">Potentially newer school object</param>
        /// <returns></returns>
        public UpdateCheck CheckIfUpdatesAreRequired(School school)
        {
            // Check to make sure that the ID matches, and return -1 if it does not
            if (this.iSchoolID != school.iSchoolID)
            {
                return UpdateCheck.NotSameObject;
            }

            // Check all properties of the objects to see if they are different
            int updates = 0;

            if (!this.GovernmentID.Equals(school.GovernmentID)) { updates++; }
            if (!this.Name.Equals(school.Name)) { updates++; }
            
            if (updates == 0)
            {
                return UpdateCheck.NoUpdateRequired;
            } else
            {
                return UpdateCheck.UpdatesRequired;
            }
        }

    }
}

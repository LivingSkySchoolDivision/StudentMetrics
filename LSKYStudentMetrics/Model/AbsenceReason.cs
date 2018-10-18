using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics
{
    public class AbsenceReason
    {
        public int ID { get; set; }
        public string Content { get; set; } 
        public bool IsExcusable { get; set; }

        public UpdateCheck CheckIfUpdatesAreRequired(AbsenceReason obj)
        {
            // Check to make sure that the ID matches, and return -1 if it does not
            if (this.ID != obj.ID)
            {
                return UpdateCheck.NotSameObject;
            }

            // Check all properties of the objects to see if they are different
            int updates = 0;

            if (!this.Content.Equals(obj.Content)) { updates++; }

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

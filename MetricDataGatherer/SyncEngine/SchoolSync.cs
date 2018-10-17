using LSKYStudentMetrics;
using LSKYStudentMetrics.Repositories.Internal;
using LSKYStudentMetrics.Repositories.SchoolLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.SyncEngine
{
    // SCHOOLS
    // - Get a list of schools we already know about
    // - Get a list of schools in the SL database
    // - Update schools that exist, insert schools that don't

    public delegate void LogDelegate(string msg);
    static class SchoolSync
    {
        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= SCHOOLS ========= ");
            InternalSchoolRepository schoolRepo_Internal = new InternalSchoolRepository(configFile.DatabaseConnectionString_Internal);
            SLSchoolRepository schoolRepo_SL = new SLSchoolRepository(configFile.DatabaseConnectionString_SchoolLogic);

            //List<School> knownSchools = schoolRepo_Internal.GetAll();
            List<School> foundSchools = schoolRepo_SL.GetAll();

            Log("We currently know about " + schoolRepo_Internal.GetAllKnownSchoolIDs().Count() + " schools");
            Log("Found " + foundSchools.Count() + " schools in the external database");

            // Find previously unknown schools
            // Find schools that need an update
            List<School> previouslyUnknownSchools = new List<School>();
            List<School> schoolsNeedingUpdate = new List<School>();


            foreach (School foundSchool in foundSchools)
            {
                // Check to see if we know about this school already
                School thisInternalSchool = schoolRepo_Internal.Get(foundSchool.iSchoolID);
                if (thisInternalSchool == null)
                {
                    previouslyUnknownSchools.Add(foundSchool);
                }

                // Check to see if this school requires an update
                if (thisInternalSchool != null)
                {
                    UpdateCheck check = thisInternalSchool.CheckIfUpdatesAreRequired(foundSchool);                    
                    if ((check == UpdateCheck.UpdatesRequired) || (forceUpdate))
                    {
                        Log("Update required for " + foundSchool.iSchoolID);
                        schoolsNeedingUpdate.Add(foundSchool);
                    }
                }
            }

            // Find schools that are no longer in the database that could potentially be cleaned up
            List<int> foundSchoolIDs = schoolRepo_SL.GetAllKnownSchoolIDs();
            List<School> schoolsThatNoLongerExist = new List<School>();
            foreach (School existingSchool in schoolRepo_Internal.GetAll())
            {
                if (!foundSchoolIDs.Contains(existingSchool.iSchoolID))
                {
                    schoolsThatNoLongerExist.Add(existingSchool);
                }
            }

            Log("Found " + previouslyUnknownSchools.Count() + " previously unknown schools");
            Log("Found " + schoolsNeedingUpdate.Count() + " schools with updates");
            Log("Found " + schoolsThatNoLongerExist.Count() + " schools not in external database");

            // Commit these changes to the database
            foreach (School school in previouslyUnknownSchools)
            {
                Log(" > Adding new school: " + school.Name);
                schoolRepo_Internal.Add(school);
            }

            foreach (School school in schoolsNeedingUpdate)
            {
                Log(" > Updating school: " + school.Name);
                schoolRepo_Internal.Update(school);
            }

            // Remove from the database here, but we don't currently care about that               
            
        }


    }
}

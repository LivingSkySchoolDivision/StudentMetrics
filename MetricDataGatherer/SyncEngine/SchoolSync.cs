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

    static class SchoolSync
    {
        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= SCHOOLS ========= ");
            InternalSchoolRepository internalRepository = new InternalSchoolRepository(configFile.DatabaseConnectionString_Internal);
            SLSchoolRepository externalRepository = new SLSchoolRepository(configFile.DatabaseConnectionString_SchoolLogic);
                        
            List<School> foundSchools = externalRepository.GetAll();

            Log("Found " + internalRepository.GetAllKnownSchoolIDs().Count() + " schools in internal database");
            Log("Found " + foundSchools.Count() + " schools in external database");

            // Find previously unknown schools
            // Find schools that need an update
            List<School> previouslyUnknown = new List<School>();
            List<School> needingUpdate = new List<School>();
            List<School> noLongerExistsInExternalSystem = new List<School>();

            foreach (School externalObject in foundSchools)
            {
                // Check to see if we know about this school already
                School internalObject = internalRepository.Get(externalObject.iSchoolID);
                if (internalObject == null)
                {
                    previouslyUnknown.Add(externalObject);
                }

                // Check to see if this school requires an update
                if (internalObject != null)
                {
                    UpdateCheck check = internalObject.CheckIfUpdatesAreRequired(externalObject);                    
                    if ((check == UpdateCheck.UpdatesRequired) || (forceUpdate))
                    {
                        Log("Update required for " + externalObject.iSchoolID);
                        needingUpdate.Add(externalObject);
                    }
                }
            }

            // Find schools that are no longer in the database that could potentially be cleaned up
            List<int> foundIDs = externalRepository.GetAllIDs();            
            foreach (School internalObject in internalRepository.GetAll())
            {
                if (!foundIDs.Contains(internalObject.iSchoolID))
                {
                    noLongerExistsInExternalSystem.Add(internalObject);
                }
            }

            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");
            Log("Found " + noLongerExistsInExternalSystem.Count() + " not in external database");

            // Commit these changes to the database
            foreach (School school in previouslyUnknown)
            {
                Log(" > Adding new school: " + school.Name);
                internalRepository.Add(school);
            }

            foreach (School school in needingUpdate)
            {
                Log(" > Updating school: " + school.Name);
                internalRepository.Update(school);
            }

            // Remove from the database here, but we don't currently care about that               
            
        }


    }
}

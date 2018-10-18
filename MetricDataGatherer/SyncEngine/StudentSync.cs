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
    static class StudentSync
    {
        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= STUDENTS ========= ");

            SLStudentRepository externalRepository = new SLStudentRepository(configFile.DatabaseConnectionString_SchoolLogic);
            InternalStudentRepository internalRepository = new InternalStudentRepository(configFile.DatabaseConnectionString_Internal);

            List<Student> objectsInThirdPartySystem = externalRepository.GetAll();

            List<Student> previouslyUnknown = new List<Student>();
            List<Student> needingUpdate = new List<Student>();
            List<Student> noLongerExistsInExternalSystem = new List<Student>();

            foreach(Student externalObject in objectsInThirdPartySystem)
            {
                // Objects we don't have in the database
                Student internalObject = internalRepository.Get(externalObject.iStudentID);
                if (internalObject == null)
                {
                    previouslyUnknown.Add(externalObject);
                }

                // Objects requiring update
                if (internalObject != null)
                {
                    UpdateCheck check = internalObject.CheckIfUpdatesAreRequired(externalObject);
                    if ((check == UpdateCheck.UpdatesRequired) || (forceUpdate))
                    {
                        Log("Update required for " + internalObject.iStudentID);
                        needingUpdate.Add(externalObject);
                    }
                }                
            }

            // Objects in the internal database that aren't in the external database
            List<int> foundIDs = externalRepository.GetAllIDs();
            foreach (Student internalObject in internalRepository.GetAll())
            {
                if (!foundIDs.Contains(internalObject.iStudentID))
                {
                    noLongerExistsInExternalSystem.Add(internalObject);
                }
            }

            Log("Found " + internalRepository.GetAll().Count() + " students in internal database");
            Log("Found " + objectsInThirdPartySystem.Count() + " students in external database");

            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");
            Log("Found " + noLongerExistsInExternalSystem.Count() + " not in external database");

            // Commit these changes to the database
            Log("Processing " + previouslyUnknown.Count() + " adds...");
            foreach (Student obj in previouslyUnknown)
            {
                internalRepository.Add(obj);
            }

            Log("Processing " + needingUpdate.Count() + " updates...");
            foreach (Student obj in needingUpdate)
            {
                internalRepository.Update(obj);
            }


            // Remove from the database here, but we don't currently care about that      
        }
    }
}

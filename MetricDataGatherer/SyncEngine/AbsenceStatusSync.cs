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
    class AbsenceStatusSync
    {

        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= ABSENCE STATUSES ========= ");
            InternalAbsenceStatusRepository internalRepository = new InternalAbsenceStatusRepository(configFile.DatabaseConnectionString_Internal);
            SLAbsenceStatusRepository externalRepository = new SLAbsenceStatusRepository(configFile.DatabaseConnectionString_SchoolLogic);

            List<AbsenceStatus> externalObjects = externalRepository.GetAll();

            Log("Found " + internalRepository.GetAllIDs().Count() + " statuses in internal database");
            Log("Found " + externalObjects.Count() + " statuses in external database");

            // Find previously unknown
            // Find objects that need an update
            List<AbsenceStatus> previouslyUnknown = new List<AbsenceStatus>();
            List<AbsenceStatus> needingUpdate = new List<AbsenceStatus>();
            List<AbsenceStatus> noLongerExistsInExternalSystem = new List<AbsenceStatus>();

            foreach (AbsenceStatus externalObject in externalObjects)
            {
                // Check to see if we know about this school already
                AbsenceStatus internalObject = internalRepository.Get(externalObject.ID);
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
                        Log("Update required for " + externalObject.ID);
                        needingUpdate.Add(externalObject);
                    }
                }
            }

            // Find schools that are no longer in the database that could potentially be cleaned up
            List<int> foundIDs = externalRepository.GetAllIDs();
            foreach (AbsenceStatus internalObject in internalRepository.GetAll())
            {
                if (!foundIDs.Contains(internalObject.ID))
                {
                    noLongerExistsInExternalSystem.Add(internalObject);
                }
            }

            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");
            Log("Found " + noLongerExistsInExternalSystem.Count() + " not in external database");

            // Commit these changes to the database
            foreach (AbsenceStatus school in previouslyUnknown)
            {
                Log(" > Adding new status: " + school.Content);
                internalRepository.Add(school);
            }

            foreach (AbsenceStatus school in needingUpdate)
            {
                Log(" > Updating status: " + school.Content);
                internalRepository.Update(school);
            }

            // Remove from the database here, but we don't currently care about that               

        }
    }
}

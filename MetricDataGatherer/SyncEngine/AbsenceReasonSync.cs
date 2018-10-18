using LSKYStudentMetrics;
using System;
using System.Collections.Generic;
using LSKYStudentMetrics.Repositories.SchoolLogic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSKYStudentMetrics.Repositories.Internal;

namespace MetricDataGatherer.SyncEngine
{
    class AbsenceReasonSync
    {

        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= ABSENCE REASONS ========= ");
            InternalAbsenceReasonRepository internalRepository = new InternalAbsenceReasonRepository(configFile.DatabaseConnectionString_Internal);
            SLAbsenceReasonRepository externalRepository = new SLAbsenceReasonRepository(configFile.DatabaseConnectionString_SchoolLogic);

            List<AbsenceReason> externalObjects = externalRepository.GetAll();

            Log("Found " + internalRepository.GetAllIDs().Count() + " reasons in internal database");
            Log("Found " + externalObjects.Count() + " reasons in external database");

            // Find previously unknown schools
            // Find schools that need an update
            List<AbsenceReason> previouslyUnknown = new List<AbsenceReason>();
            List<AbsenceReason> needingUpdate = new List<AbsenceReason>();
            List<AbsenceReason> noLongerExistsInExternalSystem = new List<AbsenceReason>();

            foreach (AbsenceReason externalObject in externalObjects)
            {
                // Check to see if we know about this school already
                AbsenceReason internalObject = internalRepository.Get(externalObject.ID);
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
            foreach (AbsenceReason internalObject in internalRepository.GetAll())
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
            foreach (AbsenceReason school in previouslyUnknown)
            {
                Log(" > Adding new reason: " + school.Content);
                internalRepository.Add(school);
            }

            foreach (AbsenceReason school in needingUpdate)
            {
                Log(" > Updating reason: " + school.Content);
                internalRepository.Update(school);
            }

            // Remove from the database here, but we don't currently care about that               

        }
    }
}

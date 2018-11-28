using LSSDMetricsLibrary;
using System;
using System.Collections.Generic;
using LSSDMetricsLibrary.Repositories.SchoolLogic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSSDMetricsLibrary.Repositories.Internal;

namespace MetricDataGatherer.SyncEngine
{
    class AbsenceReasonSync
    {

        public static void Sync(ConfigFile configFile, LogDelegate Log)
        {
            ConfigFileSyncPermissionsSection config = configFile.AbsenceReasonPermissions
                ;
            Log("========= ABSENCE REASONS ========= ");
            if (!config.AllowSync)
            {
                Log("This sync module is disabled in config file - skipping");
                return;
            }
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
                    if ((check == UpdateCheck.UpdatesRequired) || (config.ForceUpdate))
                    {
                        needingUpdate.Add(externalObject);
                    }
                }
            }

            // Find schools that are no longer in the database that could potentially be cleaned up
            if (config.AllowRemovals)
            {
                List<int> foundIDs = externalRepository.GetAllIDs();
                foreach (AbsenceReason internalObject in internalRepository.GetAll())
                {
                    if (!foundIDs.Contains(internalObject.ID))
                    {
                        noLongerExistsInExternalSystem.Add(internalObject);
                    }
                }
            }

            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");
            Log("Found " + noLongerExistsInExternalSystem.Count() + " not in external database");

            // Commit these changes to the database
            if (previouslyUnknown.Count > 0)
            {
                if (config.AllowAdds)
                {
                    Log(" > Adding " + previouslyUnknown.Count() + " new objects");
                    internalRepository.Add(previouslyUnknown);
                }
                else
                {
                    Log(" > Not allowed to add, skipping " + previouslyUnknown.Count() + " adds");

                }
            }


            if (needingUpdate.Count > 0)
            {
                if (config.AllowUpdates)
                {
                    Log(" > Updating " + needingUpdate.Count() + " objects");
                    internalRepository.Update(needingUpdate);
                }
                else
                {
                    Log(" > Not allowed to do updates, skipping " + needingUpdate.Count() + " updates");
                }
            }

            if (noLongerExistsInExternalSystem.Count > 0)
            {
                if (config.AllowRemovals)
                {
                    Log(" > If removals were implemented, we would remove " + noLongerExistsInExternalSystem.Count() + " objects here");
                }
                else
                {
                    Log(" > Not allowed to remove, skipping " + noLongerExistsInExternalSystem.Count() + " removals");
                }
            }
        }
    }
}

using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Extensions;
using LSSDMetricsLibrary.Repositories.Internal;
using LSSDMetricsLibrary.Repositories.SchoolLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.SyncEngine
{
    class AbsenceSync
    {
        public static void Sync(ConfigFile configFile, LogDelegate Log)
        {
            ConfigFileSyncPermissionsSection config = configFile.AbsencePermissions;
            Log("========= ABSENCES ========= ");
            if (!config.AllowSync)
            {
                Log("This sync module is disabled in config file - skipping");
                return;
            }

            // Parse the school year from the config file, we'll need it later            
            InternalSchoolYearRepository _schoolYearRepo = new InternalSchoolYearRepository(configFile.DatabaseConnectionString_Internal);
            SchoolYear schoolYear = _schoolYearRepo.Get(configFile.SchoolYearName);
            if (schoolYear == null)
            {
                throw new InvalidSchoolYearException("School year from config file is invalid");
            }
            SLAbsenceRepository externalRepository = new SLAbsenceRepository(configFile.DatabaseConnectionString_SchoolLogic, schoolYear);
            InternalAbsenceRepository internalRepository = new InternalAbsenceRepository(configFile.DatabaseConnectionString_Internal);
            
            List<Absence> externalObjects = externalRepository.GetAll();
            List<Absence> internalObjects = internalRepository.GetForSchoolYear(schoolYear.ID);

            Log("Found " + internalObjects.Count() + " absences in internal database for this school year");
            Log("Found " + externalObjects.Count() + " absences in external database");

            // Find previously unknown schools
            // Find schools that need an update
            List<Absence> previouslyUnknown = new List<Absence>();
            List<Absence> needingUpdate = new List<Absence>();
            List<Absence> noLongerExistsInExternalSystem = new List<Absence>();

            int doneCount = 0;
            int totalExternalObjects = externalObjects.Count();
            decimal donePercent = 0;
            decimal doneThresholdPercent = (decimal)0.01;
            decimal doneThresholdIncrease = (decimal)0.01;
            foreach (Absence externalObject in externalObjects)
            {
                // Check to see if we know about this school already
                Absence internalObject = internalObjects.GetWithID(externalObject.ID);
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

                doneCount++;
                donePercent = (decimal)((decimal)doneCount / (decimal)totalExternalObjects);                
                if (donePercent > doneThresholdPercent)
                {
                    doneThresholdPercent = doneThresholdPercent + doneThresholdIncrease;
                    Log((int)(donePercent*100) + "% finished inspecting objects");
                }

                if (doneCount == totalExternalObjects)
                {
                    Log("100% finished inspecting objects");
                }
            }

            // Find schools that are no longer in the database that could potentially be cleaned up
            if (config.AllowRemovals)
            {
                List<int> foundIDs = externalRepository.GetAllIDs();
                foreach (Absence internalObject in internalObjects)
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
                } else
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
                } else
                {
                    Log(" > Not allowed to do updates, skipping " + needingUpdate.Count() + " updates");
                }
            }

            if (noLongerExistsInExternalSystem.Count > 0)
            {
                if (config.AllowRemovals)
                {
                    Log(" > If removals were implemented, we would remove " + noLongerExistsInExternalSystem.Count() + " objects here");
                } else
                {
                    Log(" > Not allowed to remove, skipping " + noLongerExistsInExternalSystem.Count() + " removals");
                }
            }

            
        }
    }
}

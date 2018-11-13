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
    class StudentGradePlacementSync
    {
        public static void Sync(ConfigFile configFile, LogDelegate Log)
        {
            ConfigFileSyncPermissionsSection config = configFile.StudentGradePlacementPermissions;

            Log("========= GRADE PLACEMENTS FOR " + configFile.SchoolYearName + " ========= ");
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
            InternalGradePlacementRepository internalRepository = new InternalGradePlacementRepository(configFile.DatabaseConnectionString_Internal);
            
            SLGradePlacementRepository externalRepository = new SLGradePlacementRepository(configFile.DatabaseConnectionString_SchoolLogic, schoolYear);            

            // This one is handled differently than other syncs
              
            List<StudentGradePlacement> externalObjects = externalRepository.GetAll();

            Log("Found " + internalRepository.GetAllForSchoolYear(schoolYear.ID).Count() + " placements in internal database for this school year");
            Log("Found " + externalObjects.Count() + " placements in external database");

            // Find previously unknown schools
            // Find schools that need an update
            List<StudentGradePlacement> previouslyUnknown = new List<StudentGradePlacement>();
            List<StudentGradePlacement> needingUpdate = new List<StudentGradePlacement>();
            List<StudentGradePlacement> noLongerExistsInExternalSystem = new List<StudentGradePlacement>();

            int doneCount = 0;
            int totalExternalObjects = externalObjects.Count();
            decimal donePercent = 0;
            decimal doneThresholdPercent = (decimal)0.1;
            decimal doneThresholdIncrease = (decimal)0.1;
            foreach (StudentGradePlacement externalObject in externalObjects)
            {
                // Check to see if we know about this object already                
                StudentGradePlacement internalObject = internalRepository.Get(schoolYear, externalObject.iStudentID);
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
                    Log((int)(donePercent * 100) + "% finished inspecting objects");
                }

                if (doneCount == totalExternalObjects)
                {
                    Log("100% finished inspecting objects");
                }
            }
            
            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");

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

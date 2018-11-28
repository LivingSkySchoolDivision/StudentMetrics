using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Repositories.Internal;
using LSSDMetricsLibrary.Repositories.SchoolLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.SyncEngine
{
    static class StudentSchoolEnrolmentSync
    {
        public static void Sync(ConfigFile configFile, LogDelegate Log)
        {
            ConfigFileSyncPermissionsSection config = configFile.StudentSchoolEnrolmentPermissions;

            Log("========= STUDENT SCHOOL ENROLMENTS ========= ");
            if (!config.AllowSync)
            {
                Log("This sync module is disabled in config file - skipping");
                return;
            }

            SLStudentSchoolEnrolmentRepository externalRepository = new SLStudentSchoolEnrolmentRepository(configFile.DatabaseConnectionString_SchoolLogic);
            InternalStudentSchoolEnrolmentRepository internalRepository = new InternalStudentSchoolEnrolmentRepository(configFile.DatabaseConnectionString_Internal);

            List<StudentSchoolEnrolment> externalObjects = externalRepository.GetAll();

            List<StudentSchoolEnrolment> previouslyUnknown = new List<StudentSchoolEnrolment>();
            List<StudentSchoolEnrolment> needingUpdate = new List<StudentSchoolEnrolment>();
            List<StudentSchoolEnrolment> noLongerExistsInExternalSystem = new List<StudentSchoolEnrolment>();

            int doneCount = 0;
            int totalExternalObjects = externalObjects.Count();
            decimal donePercent = 0;
            decimal doneThresholdPercent = (decimal)0.1;
            decimal doneThresholdIncrease = (decimal)0.1;           

            foreach (StudentSchoolEnrolment externalObject in externalObjects)
            {
                // Objects we don't have in the database
                StudentSchoolEnrolment internalObject = internalRepository.Get(externalObject.ID);                
                if (internalObject == null)
                {
                    previouslyUnknown.Add(externalObject);
                }

                // Objects requiring update
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

            // Objects in the internal database that aren't in the external database
            if (config.AllowRemovals)
            {
                List<int> foundIDs = externalRepository.GetAllIDs();
                foreach (StudentSchoolEnrolment internalObject in internalRepository.GetAll())
                {
                    if (!foundIDs.Contains(internalObject.iStudentID))
                    {
                        noLongerExistsInExternalSystem.Add(internalObject);
                    }
                }
            }

            Log("Found " + internalRepository.GetAll().Count() + " objects in internal database");
            Log("Found " + externalObjects.Count() + " objects in external database");

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

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
        public static void Sync(ConfigFile configFile, bool allowAdds, bool allowUpdates, bool allowRemovals, bool forceUpdate, LogDelegate Log)
        {
            Log("========= STUDENTS ========= ");

            SLStudentRepository externalRepository = new SLStudentRepository(configFile.DatabaseConnectionString_SchoolLogic);
            InternalStudentRepository internalRepository = new InternalStudentRepository(configFile.DatabaseConnectionString_Internal);

            List<Student> externalObjects = externalRepository.GetAll();

            List<Student> previouslyUnknown = new List<Student>();
            List<Student> needingUpdate = new List<Student>();
            List<Student> noLongerExistsInExternalSystem = new List<Student>();

            int doneCount = 0;
            int totalExternalObjects = externalObjects.Count();
            decimal donePercent = 0;
            decimal doneThresholdPercent = (decimal)0.1;
            decimal doneThresholdIncrease = (decimal)0.1;
            foreach (Student externalObject in externalObjects)
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
            if (allowRemovals)
            {
                List<int> foundIDs = externalRepository.GetAllIDs();
                foreach (Student internalObject in internalRepository.GetAll())
                {
                    if (!foundIDs.Contains(internalObject.iStudentID))
                    {
                        noLongerExistsInExternalSystem.Add(internalObject);
                    }
                }
            }

            Log("Found " + internalRepository.GetAll().Count() + " students in internal database");
            Log("Found " + externalObjects.Count() + " students in external database");

            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");
            Log("Found " + noLongerExistsInExternalSystem.Count() + " not in external database");

            // Commit these changes to the database
            if (previouslyUnknown.Count > 0)
            {
                if (allowAdds)
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
                if (allowUpdates)
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
                if (allowRemovals)
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

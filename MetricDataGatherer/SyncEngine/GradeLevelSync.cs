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
    class GradeLevelSync
    {
        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= GRADE LEVELS ========= ");
            InternalGradeLevelRepository internalRepository = new InternalGradeLevelRepository(configFile.DatabaseConnectionString_Internal);
            SLGradeLevelRepository externalRepository = new SLGradeLevelRepository(configFile.DatabaseConnectionString_SchoolLogic);

            List<GradeLevel> externalObjects = externalRepository.GetAll();

            Log("Found " + internalRepository.GetAllIDs().Count() + " grades in internal database");
            Log("Found " + externalObjects.Count() + " grades in external database");

            // Find previously unknown
            // Find objects that need an update
            List<GradeLevel> previouslyUnknown = new List<GradeLevel>();
            List<GradeLevel> needingUpdate = new List<GradeLevel>();
            List<GradeLevel> noLongerExistsInExternalSystem = new List<GradeLevel>();

            foreach (GradeLevel externalObject in externalObjects)
            {
                // Check to see if we know about this school already
                GradeLevel internalObject = internalRepository.Get(externalObject.ID);
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
            foreach (GradeLevel internalObject in internalRepository.GetAll())
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
            foreach (GradeLevel obj in previouslyUnknown)
            {
                Log(" > Adding new grade: " + obj.Name);
                internalRepository.Add(obj);
            }

            foreach (GradeLevel obj in needingUpdate)
            {
                Log(" > Updating grade: " + obj.Name);
                internalRepository.Update(obj);
            }

            // Remove from the database here, but we don't currently care about that               

        }
    }
}

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
        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= GRADE PLACEMENTS FOR " + configFile.SchoolYearName + " ========= ");
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
                    if ((check == UpdateCheck.UpdatesRequired) || (forceUpdate))
                    {                        
                        needingUpdate.Add(externalObject);
                    }
                }
            }
            
            Log("Found " + previouslyUnknown.Count() + " previously unknown");
            Log("Found " + needingUpdate.Count() + " with updates");

            // Commit these changes to the database
            if (previouslyUnknown.Count > 0)
            {
                Log(" > Committing " + previouslyUnknown.Count() + " new grade placements...");
                internalRepository.Add(previouslyUnknown);
            }

            if (needingUpdate.Count > 0)
            {
                Log(" > Updating " + needingUpdate.Count() + " grade placements...");
                internalRepository.Update(needingUpdate);
            }

            
            //*/
        }
    }
}

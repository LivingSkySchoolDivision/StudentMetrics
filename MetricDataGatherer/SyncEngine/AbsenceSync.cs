using LSKYStudentMetrics;
using LSKYStudentMetrics.Extensions;
using LSKYStudentMetrics.Repositories.Internal;
using LSKYStudentMetrics.Repositories.SchoolLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer.SyncEngine
{
    class AbsenceSync
    {
        public static void Sync(ConfigFile configFile, bool forceUpdate, LogDelegate Log)
        {
            Log("========= ABSENCES ========= ");
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
                    if ((check == UpdateCheck.UpdatesRequired) || (forceUpdate))
                    {
                        Log("Update required for " + externalObject.ID);
                        needingUpdate.Add(externalObject);
                    }
                }
            }

            // Find schools that are no longer in the database that could potentially be cleaned up
            List<int> foundIDs = externalRepository.GetAllIDs();
            foreach (Absence internalObject in internalObjects)
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
            if (previouslyUnknown.Count > 0)
            {
                Log(" > Committing " + previouslyUnknown.Count() + " new absences...");
                internalRepository.Add(previouslyUnknown);
            }

            if (needingUpdate.Count > 0)
            {
                Log(" > Updating " + needingUpdate.Count() + " absences...");
                internalRepository.Update(needingUpdate);
            }

            // Remove from the database here, but we don't currently care about that               
            //*/
        }
    }
}

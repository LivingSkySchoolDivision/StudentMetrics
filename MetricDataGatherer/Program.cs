using LSKYStudentMetrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSKYStudentMetrics.Repositories;
using LSKYStudentMetrics.Repositories.SchoolLogic;
using LSKYStudentMetrics.Repositories.Internal;
using MetricDataGatherer.SyncEngine;

namespace MetricDataGatherer
{
    class Program
    {
        public static void Log(string msg)
        {
            Console.WriteLine(DateTime.Now + ": " + msg);
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Any())
                {
                    string configFileName = string.Empty;
                    
                    foreach (string argument in args)
                    {
                        if (argument.ToLower().StartsWith("/config:"))
                        {
                            configFileName = argument.Substring(8, argument.Length - 8);
                        }
                    }

                    if (string.IsNullOrEmpty(configFileName))
                    {
                        throw new SyntaxException("A config file filename is required");
                    }

                    // Attempt to load the config file
                    ConfigFile configFile = ConfigFile.LoadFromFile(configFileName);
                    
                    // Validate the config file
                    configFile.Validate();

                    // Parse the working school year
                    InternalSchoolYearRepository schoolYearRepository = new InternalSchoolYearRepository(configFile.DatabaseConnectionString_Internal);
                    SchoolYear schoolYear = schoolYearRepository.Get(configFile.SchoolYearName);

                    if (schoolYear == null) { throw new Exception("Invalid school year in config file: " + configFile.SchoolYearName); }

                    Log("Config file checks OK");
                    // Display information on what we're allowed to do based on the config file
                    Log("Permissions summary:");                    
                    Log("Absences: \t\t" + configFile.AbsencePermissions.ToString());
                    Log("AbsenceStatuses: \t" + configFile.AbsenceStatusPermissions.ToString());
                    Log("AbsenceReasons: \t\t" + configFile.AbsenceReasonPermissions.ToString());
                    Log("GradeLevels: \t\t" + configFile.GradeLevelPermissions.ToString());
                    Log("Schools: \t\t" + configFile.SchoolPermissions.ToString());
                    Log("StudentGradePlacements: \t" + configFile.StudentGradePlacementPermissions.ToString());
                    Log("Students: \t\t" + configFile.StudentPermissions.ToString());


                    LogDelegate logCallback = Log;

                    // Conduct the sync

                    // Check to see if the school year specified in the config file is a valid one

                    // SCHOOLS
                    SchoolSync.Sync(configFile, configFile.SchoolPermissions.AllowAdds, configFile.SchoolPermissions.AllowUpdates, configFile.SchoolPermissions.AllowRemovals, configFile.SchoolPermissions.ForceUpdate, logCallback);

                    // GRADE LEVELS
                    GradeLevelSync.Sync(configFile, configFile.GradeLevelPermissions.AllowAdds, configFile.GradeLevelPermissions.AllowUpdates, configFile.GradeLevelPermissions.AllowRemovals, configFile.GradeLevelPermissions.ForceUpdate, logCallback);

                    // GRADE PLACEMENTS
                    StudentGradePlacementSync.Sync(configFile, configFile.StudentGradePlacementPermissions.AllowAdds, configFile.StudentGradePlacementPermissions.AllowUpdates, configFile.StudentGradePlacementPermissions.AllowRemovals, configFile.StudentGradePlacementPermissions.ForceUpdate, logCallback);

                    // STUDENTS
                    StudentSync.Sync(configFile, configFile.StudentPermissions.AllowAdds, configFile.StudentPermissions.AllowUpdates, configFile.StudentPermissions.AllowRemovals, configFile.StudentPermissions.ForceUpdate, logCallback);

                    // ABSENCE REASONS
                    AbsenceReasonSync.Sync(configFile, configFile.AbsenceReasonPermissions.AllowAdds, configFile.AbsenceReasonPermissions.AllowUpdates, configFile.AbsenceReasonPermissions.AllowRemovals, configFile.AbsenceReasonPermissions.ForceUpdate, logCallback);

                    // ABSENCE STATUSES
                    AbsenceStatusSync.Sync(configFile, configFile.AbsenceStatusPermissions.AllowAdds, configFile.AbsenceStatusPermissions.AllowUpdates, configFile.AbsenceStatusPermissions.AllowRemovals, configFile.AbsenceStatusPermissions.ForceUpdate, logCallback);

                    // Absences
                    AbsenceSync.Sync(configFile, configFile.AbsencePermissions.AllowAdds, configFile.AbsencePermissions.AllowUpdates, configFile.AbsencePermissions.AllowRemovals, configFile.AbsencePermissions.ForceUpdate, logCallback);

                    // STUDENT EXPECTED BLOCKS PER DAY
                    StudentExpectedBlocksSync.Sync(configFile, logCallback);

                }
                else
                {
                    throw new SyntaxException("Not enough arguments");
                }
            }
            catch (SyntaxException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                SendSyntax();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void SendSyntax()
        {
            Console.WriteLine("SYNTAX:");
            Console.WriteLine("");
            Console.WriteLine(" REQUIRED:");
            Console.WriteLine("  /config:filename.conf");
            Console.WriteLine("  Specify the file name");
        }
    }
}

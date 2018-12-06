using LSSDMetricsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSSDMetricsLibrary.Repositories;
using LSSDMetricsLibrary.Repositories.SchoolLogic;
using LSSDMetricsLibrary.Repositories.Internal;
using MetricDataGatherer.SyncEngine;
using System.IO;

namespace MetricDataGatherer
{
    class Program
    {
        static ConfigFile configFile = new ConfigFile();

        public static void Log(string msg)
        {
            if (configFile.Loaded)
            {
                using (StreamWriter w = File.AppendText(configFile.LogDirectory + "/" + configFile.LogFileName))
                {
                    w.WriteLine(DateTime.Now + ": " + msg);
                }
                Console.WriteLine(DateTime.Now + ": " + msg);
            }
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
                    configFile = ConfigFile.LoadFromFile(configFileName);
                    
                    // Validate the config file
                    configFile.Validate();

                    Log("Data Gatherer started");

                    // Check to see if the log file folder needs to be created
                    if (!Directory.Exists(configFile.LogDirectory))
                    {
                        Directory.CreateDirectory(configFile.LogDirectory);
                    }

                    // Parse the working school year
                    InternalSchoolYearRepository schoolYearRepository = new InternalSchoolYearRepository(configFile.DatabaseConnectionString_Internal);
                    SchoolYear schoolYear = schoolYearRepository.Get(configFile.SchoolYearName);

                    if (schoolYear == null) { throw new Exception("Invalid school year in config file: " + configFile.SchoolYearName); }

                    Log("Config file checks OK");
                    // Display information on what we're allowed to do based on the config file
                    Log("Permissions summary:");                    
                    Log("Absences: \t\t\t" + configFile.AbsencePermissions.ToString());
                    Log("AbsenceStatuses: \t\t" + configFile.AbsenceStatusPermissions.ToString());
                    Log("AbsenceReasons: \t\t" + configFile.AbsenceReasonPermissions.ToString());
                    Log("GradeLevels: \t\t\t" + configFile.GradeLevelPermissions.ToString());
                    Log("Schools: \t\t\t" + configFile.SchoolPermissions.ToString());
                    Log("StudentGradePlacements: \t" + configFile.StudentGradePlacementPermissions.ToString());
                    Log("Students: \t\t\t" + configFile.StudentPermissions.ToString());
                    Log("ExpectedAttendance: \t\t" + configFile.ExpectedAttendancePermissions.ToString());
                    Log("StudentSchoolEnrolments: \t\t" + configFile.StudentSchoolEnrolmentPermissions.ToString());

                    LogDelegate logCallback = Log;

                    // Conduct the sync

                    // SCHOOLS
                    SchoolSync.Sync(configFile, logCallback);

                    // GRADE LEVELS
                    GradeLevelSync.Sync(configFile, logCallback);

                    // GRADE PLACEMENTS
                    StudentGradePlacementSync.Sync(configFile, logCallback);

                    // STUDENTS
                    StudentSync.Sync(configFile, logCallback);

                    // ABSENCE REASONS
                    AbsenceReasonSync.Sync(configFile, logCallback);

                    // ABSENCE STATUSES
                    AbsenceStatusSync.Sync(configFile, logCallback);

                    // Absences
                    AbsenceSync.Sync(configFile, logCallback);

                    // STUDENT EXPECTED BLOCKS PER DAY
                    StudentExpectedBlocksSync.Sync(configFile, logCallback);

                    // Student school enrolments
                    StudentSchoolEnrolmentSync.Sync(configFile, logCallback);

                    Log("Data Gatherer complete");
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
            }// */
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

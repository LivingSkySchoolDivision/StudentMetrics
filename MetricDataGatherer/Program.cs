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

                    LogDelegate logCallback = Log;

                    // Conduct the sync

                    // Check to see if the school year specified in the config file is a valid one

                    // SCHOOLS
                    SchoolSync.Sync(configFile, false, logCallback);

                    // GRADE LEVELS
                    GradeLevelSync.Sync(configFile, false, logCallback);

                    // STUDENTS
                    StudentSync.Sync(configFile, false, logCallback);

                    // ABSENCE REASONS
                    AbsenceReasonSync.Sync(configFile, false, logCallback);

                    // ABSENCE STATUSES
                    AbsenceStatusSync.Sync(configFile, false, logCallback);


                    // STUDENT GRADE LEVELS
                    // Remembering to account for the currently selected school year

                    // Absences




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

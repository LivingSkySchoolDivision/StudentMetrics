using LSKYStudentMetrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricDataGatherer
{
    class Program
    {        
        private static void Log(string msg)
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
                    SchoolYearRepository schoolYearRepository = new SchoolYearRepository(configFile.DatabaseConnectionString_Internal);
                    SchoolYear schoolYear = schoolYearRepository.Get(configFile.SchoolYearName);

                    if (schoolYear == null) { throw new Exception("Invalid school year in config file: " + configFile.SchoolYearName); }

                    Log("Config file checks OK");



                    // Conduct the sync

                    // SCHOOLS
                    // - Get a list of schools we already know about
                    // - Get a list of schools in the SL database
                    // - Update schools that exist, insert schools that don't

                    Log("Syncing schools...");
                    List<int> knownSchoolIDs = new List<int>();
                    List<int> foundSchoolIDs = new List<int>();



                    // STUDENTS

                    // STUDENT GRADE LEVELS




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

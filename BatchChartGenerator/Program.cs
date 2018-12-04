using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchChartGenerator
{
    class Program
    {
        static string _configFileName = "Config.xml";
        static string _jobsDirectory = "jobs/";
        static string _chartOutputDirectory = "output/";

        public static void Log(string msg)
        {
            Console.WriteLine(DateTime.Now + ": " + msg);
        }

        static void SendSyntax()
        {
            Console.WriteLine("SYNTAX:");
            Console.WriteLine("  /config:filename.xml\t\tSpecify the file name to use as a configuration file");
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Any())
                {

                    foreach (string argument in args)
                    {
                        if (argument.ToLower().StartsWith("/config:"))
                        {
                            _configFileName = argument.Substring(8, argument.Length - 8);
                        }

                        if (argument.ToLower().StartsWith("/?"))
                        {
                            throw new SyntaxException("");
                        }
                    }
                }

                // Attempt to load the config file
                ConfigFile configFile = ConfigFile.LoadFromFile(_configFileName);

                // Validate the config file
                configFile.Validate();

                // Check the jobs directory to see if it exists
                // If it doesn't, create it

                // Check the output directory to see if it exists
                // If it doesn't, create it
                
                // Attempt to deserialize config files from the jobs folder



            }
            catch (SyntaxException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                SendSyntax();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }// */
        }
    }
}

using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Extensions;
using LSSDMetricsLibrary.Helpers;
using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BatchChartGenerator
{
    class Program
    {
        static string _configFileName = "Config.xml";
        static XmlSerializer _chartJobSerializer = new XmlSerializer(typeof(ChartJob));
        static ConfigFile _configFile = new ConfigFile();

        public static void Log(string msg)
        {
            Console.WriteLine(DateTime.Now + ": " + msg);
        }

        static void SendSyntax()
        {
            Console.WriteLine("SYNTAX:");
            Console.WriteLine("  /config:filename.xml\t\tSpecify the file name to use as a configuration file");
        }

        private static void saveChart(byte[] chartBytes, string fullFileName)
        {
            Log("Writing " + fullFileName);
            File.WriteAllBytes(fullFileName, chartBytes);
        }

        private static ChartJob loadSavedJob(string filename)
        {
            // Build the full path
            StringBuilder fullFilePath = new StringBuilder();
            if (!filename.StartsWith(_configFile.InputDirectory))
            {
                fullFilePath.Append(_configFile.InputDirectory);
            }            
            
            fullFilePath.Append(filename);

            // Deserialize the object and send it back
            StreamReader file = new StreamReader(fullFilePath.ToString());
            ChartJob ds = (ChartJob)_chartJobSerializer.Deserialize(file);

            if (string.IsNullOrEmpty(ds.JobName))
            {
                ds.JobName = Crypto.GetMD5(ds.StartDate.ToShortDateString() + ds.EndDate.ToShortDateString() + filename);
            }

            return ds;
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
                _configFile = ConfigFile.LoadFromFile(_configFileName);

                // Validate the config file
                _configFile.Validate();

                // Check the jobs directory to see if it exists
                // If it doesn't, create it
                if (!Directory.Exists(_configFile.InputDirectory))
                {
                    Directory.CreateDirectory(_configFile.InputDirectory);
                }

                // Check the output directory to see if it exists
                // If it doesn't, create it
                if (!Directory.Exists(_configFile.OutputDirectory))
                {
                    Directory.CreateDirectory(_configFile.OutputDirectory);
                }

                // Attempt to deserialize config files from the jobs folder
                Log("Loading jobs from: " + _configFile.InputDirectory);

                List<ChartJob> loadedJobs = new List<ChartJob>();
                foreach(string fileName in Directory.GetFiles(_configFile.InputDirectory))
                {
                    Log("> " + fileName);
                    ChartJob parsedJob = loadSavedJob(fileName);
                    if (parsedJob != null)
                    {
                        loadedJobs.Add(parsedJob);
                    }
                }

                Log("Loaded " + loadedJobs.Count + " jobs.");                

                // Generate the charts and save them in the output folder

                foreach(ChartJob job in loadedJobs)
                {
                    Log("Processing job " + job.JobName);
                    string fullFileName = _configFile.OutputDirectory + "/" + job.JobName.RemoveSpecialCharacters() + ".png";

                    byte[] fileBytes = job.Generate(_configFile.DatabaseConnectionString);

                    if (fileBytes.Length > 0) {
                        // Delete file if it already exists
                        if (File.Exists(fullFileName))
                        {
                            File.Delete(fullFileName);
                        }

                        // Make the new file
                        saveChart(fileBytes, fullFileName);
                    }
                }

                Log("Done!");
            }
            catch (SyntaxException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    Log("Error: " + ex.Message);
                }
                SendSyntax();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
    }
}

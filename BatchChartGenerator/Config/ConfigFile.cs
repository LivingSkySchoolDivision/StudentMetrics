using LSSDMetricsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BatchChartGenerator
{
    class ConfigFile
    {
        private const string _defaultLogDirectory = "Logs";
        private static string _defaultLogFileName = "ChartGen-" + DateTime.Today.Year + "-" + DateTime.Today.Month + "-" + DateTime.Today.Day + ".log";
        private const string _defaultInputDirectory = "Input";
        private const string _defaultOutputDirectory = "Output";
        private const string _defaultInputFileExtension = ".xml";        

        public string DatabaseConnectionString { get; set; }
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }        
        public string LogDirectory { get; set; }
        public string LogFileName { get; set; }
        public string jobFileExtension = _defaultInputFileExtension;

        public bool Loaded { get; set; }

        public ConfigFile()
        {
            Loaded = false;
            DatabaseConnectionString = "data source=HOSTNAME;initial catalog=DATABASE;user id=USERNAME;password=PASSWORD;Trusted_Connection=false";
            InputDirectory = _defaultInputDirectory;
            OutputDirectory = _defaultOutputDirectory;
            LogDirectory = _defaultLogDirectory;
            LogFileName = _defaultLogFileName;
            jobFileExtension = _defaultInputFileExtension;
        }

        public bool Validate()
        {
            bool problemdetected = false;            
            if (string.IsNullOrEmpty(this.DatabaseConnectionString)) { problemdetected = true; throw new InvalidConfigFileException("Missing internal database connection string"); }
            
            // Attempt to connect to the internal connection string
            try
            {
                using (SqlConnection connection = new SqlConnection(this.DatabaseConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT * FROM Schools;";
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        sqlCommand.Connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                problemdetected = true;
                throw new InvalidConfigFileException("Connection string did not work: " + ex.Message);
            }
            
            return !problemdetected;
        }
        
        public static ConfigFile LoadFromFile(string FileName)
        {
            if (!File.Exists(FileName))
            {
                throw new InvalidConfigFileException("Config file not found");
            }

            string connectionString = string.Empty;            

            // Load the file into XElement
            XElement configFile = XElement.Load(FileName);

            // Connection Strings
            List<XElement> databaseSections = configFile.Elements("Database").ToList();
            // Loop through all "Database" sections (hopefully only one)
            foreach (XElement section in databaseSections)
            {
                // Loop through each setting in this section
                foreach (XElement setting in section.Elements())
                {
                    if (setting.Name == "ConnectionString")
                    {
                        connectionString = setting.Value;
                    }
                }
            }

            // Other settings
            string inputDirectory = _defaultInputDirectory;
            string outputDirectory = _defaultOutputDirectory;
            string logDirectory = _defaultLogDirectory;
            string logFilename = _defaultLogFileName;

            List<XElement> otherSettingsSection = configFile.Elements("Other").ToList();
            // Loop through all "Other" sections (hopefully only one)
            foreach (XElement section in otherSettingsSection)
            {
                // Loop through each setting in this section
                foreach (XElement setting in section.Elements())
                {
                    if (setting.Name == "InputDirectory")
                    {
                        if (!string.IsNullOrEmpty(setting.Value))
                        {
                            inputDirectory = setting.Value;
                            if (inputDirectory.EndsWith("/"))
                            {
                                inputDirectory = inputDirectory.Substring(0, inputDirectory.Length - 1);
                            }
                        }
                    }

                    if (setting.Name == "OutputDirectory")
                    {
                        if (!string.IsNullOrEmpty(setting.Value))
                        {
                            outputDirectory = setting.Value;
                            if (outputDirectory.EndsWith("/"))
                            {
                                outputDirectory = outputDirectory.Substring(0, outputDirectory.Length - 1);
                            }
                        }
                    }

                    if (setting.Name == "LogDirectory")
                    {
                        if (!string.IsNullOrEmpty(setting.Value))
                        {
                            logDirectory = setting.Value;
                            if (logDirectory.EndsWith("/"))
                            {
                                logDirectory = logDirectory.Substring(0, logDirectory.Length - 1);
                            }
                        }
                    }

                    if (setting.Name == "LogFileName")
                    {
                        if (!string.IsNullOrEmpty(setting.Value))
                        {
                            logFilename = setting.Value;
                        }
                    }
                }
            }

            return new ConfigFile()
            {
                DatabaseConnectionString = connectionString,
                InputDirectory = inputDirectory,
                OutputDirectory = outputDirectory,
                Loaded = true,
                LogDirectory = logDirectory,
                LogFileName = logFilename
            };
        }
    }
}

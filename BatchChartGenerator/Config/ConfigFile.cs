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
        public string DatabaseConnectionString { get; set; }
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }
        
        public bool Loaded { get; set; }

        public ConfigFile()
        {
            DatabaseConnectionString = "data source=HOSTNAME;initial catalog=DATABASE;user id=USERNAME;password=PASSWORD;Trusted_Connection=false";
            InputDirectory = "Input";
            OutputDirectory = "Output";
            Loaded = false;
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
            string inputDirectory = "Input";
            string outputDirectory = "Output";

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
                                outputDirectory = inputDirectory.Substring(0, inputDirectory.Length - 1);
                            }
                        }
                    }
                }
            }

            return new ConfigFile()
            {
                DatabaseConnectionString = connectionString,
                InputDirectory = inputDirectory,
                OutputDirectory = outputDirectory,
                Loaded = true
            };
        }
    }
}

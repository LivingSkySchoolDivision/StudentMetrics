using LSKYStudentMetrics;
using MetricDataGatherer.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MetricDataGatherer
{
    class ConfigFile
    {
        public string DatabaseConnectionString_SchoolLogic { get; set; }
        public string DatabaseConnectionString_Internal { get; set; }
        public string SchoolYearName { get; set; }
        public bool Loaded { get; set; }
        public ConfigFileSyncPermissionsSection AbsenceReasonPermissions { get; set; }
        public ConfigFileSyncPermissionsSection AbsenceStatusPermissions { get; set; }
        public ConfigFileSyncPermissionsSection AbsencePermissions { get; set; }
        public ConfigFileSyncPermissionsSection GradeLevelPermissions { get; set; }
        public ConfigFileSyncPermissionsSection SchoolPermissions { get; set; }
        public ConfigFileSyncPermissionsSection StudentGradePlacementPermissions { get; set; }
        public ConfigFileSyncPermissionsSection StudentPermissions { get; set; }
        public ConfigFileSyncPermissionsSection ExpectedAttendancePermissions { get; set; }

        public ConfigFile()
        {
            DatabaseConnectionString_Internal = "data source=HOSTNAME;initial catalog=DATABASE;user id=USERNAME;password=PASSWORD;Trusted_Connection=false";
            DatabaseConnectionString_SchoolLogic = "data source=HOSTNAME;initial catalog=DATABASE;user id=USERNAME;password=PASSWORD;Trusted_Connection=false";
            SchoolYearName = "Unknown";
            Loaded = false;
        }

        public override string ToString()
        {
            return "{\n\tInternal Connection String: " + this.DatabaseConnectionString_Internal + "\n\tSchoolLogic connection string: " + this.DatabaseConnectionString_SchoolLogic + "\n\tSchool Year Identifier: " + this.SchoolYearName + "\n}";
        }


        public bool Validate()
        {
            bool problemdetected = false;
            if (string.IsNullOrEmpty(this.SchoolYearName)) { problemdetected = true; throw new InvalidConfigFileException("Missing school year identifier"); }
            if (string.IsNullOrEmpty(this.DatabaseConnectionString_Internal)) { problemdetected = true; throw new InvalidConfigFileException("Missing internal database connection string"); }
            if (string.IsNullOrEmpty(this.DatabaseConnectionString_SchoolLogic)) { problemdetected = true; throw new InvalidConfigFileException("Missing SchoolLogic connection string"); }

            // Attempt to connect to the internal connection string
            try
            {
                using (SqlConnection connection = new SqlConnection(this.DatabaseConnectionString_Internal))
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
            catch(Exception ex)
            {
                problemdetected = true;
                throw new InvalidConfigFileException("Internal connection string did not work: " + ex.Message);
            }

            // Attempt to connect to the SchoolLogic connection string
            try
            {
                using (SqlConnection connection = new SqlConnection(this.DatabaseConnectionString_SchoolLogic))
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = connection;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT * FROM School;";
                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();
                        sqlCommand.Connection.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                problemdetected = true;
                throw new InvalidConfigFileException("SchoolLogic connection string did not work: " + ex.Message);
            }


            return !problemdetected;            
        }

        private static ConfigFileSyncPermissionsSection parseSyncPermissionSection(XElement section)
        {
            ConfigFileSyncPermissionsSection returnMe = new ConfigFileSyncPermissionsSection();

            // Loop through each setting in this section
            foreach (XElement setting in section.Elements())
            {
                if (setting.Name == "AllowAdd")
                {
                    returnMe.AllowAdds = Parsers.ParseBool(setting.Value);
                }

                if (setting.Name == "AllowUpdate")
                {
                    returnMe.AllowUpdates = Parsers.ParseBool(setting.Value);
                }

                if (setting.Name == "AllowRemove")
                {
                    returnMe.AllowRemovals = Parsers.ParseBool(setting.Value);
                }

                if (setting.Name == "ForceUpdate")
                {
                    returnMe.ForceUpdate = Parsers.ParseBool(setting.Value);
                }
            }

            return returnMe;
        }


        public static ConfigFile LoadFromFile(string FileName)
        {
            if (!File.Exists(FileName))
            { 
                throw new InvalidConfigFileException("Config file not found");
            }
            
            string connectionString_Internal = string.Empty;
            string connectionString_SL = string.Empty;
            string schoolYearID = string.Empty;
            ConfigFileSyncPermissionsSection _absenceReasonPermissions = new ConfigFileSyncPermissionsSection();
            ConfigFileSyncPermissionsSection _absenceStatusPermissions = new ConfigFileSyncPermissionsSection();
            ConfigFileSyncPermissionsSection _absencePermissions = new ConfigFileSyncPermissionsSection();
            ConfigFileSyncPermissionsSection _gradeLevelPermissions = new ConfigFileSyncPermissionsSection();
            ConfigFileSyncPermissionsSection _schoolPermissions = new ConfigFileSyncPermissionsSection();
            ConfigFileSyncPermissionsSection _studentGradePlacementPermissions = new ConfigFileSyncPermissionsSection();
            ConfigFileSyncPermissionsSection _studentPermissions = new ConfigFileSyncPermissionsSection();
            ConfigFileSyncPermissionsSection _expectedAttendancePermissions = new ConfigFileSyncPermissionsSection();

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
                    if (setting.Name == "ConnectionStringInternal")
                    {
                        connectionString_Internal = setting.Value;
                    }

                    if (setting.Name == "ConnectionStringSchoolLogic")
                    {
                        connectionString_SL = setting.Value;
                    }
                }
            }

            // Sync Permissions

            // For each app permissions section
            foreach (XElement appPermissionSection in configFile.Elements("AppPermissions").ToList())
            {
                foreach (XElement section in appPermissionSection.Elements())
                {
                    if (section.Name == "Absences") { _absencePermissions = parseSyncPermissionSection(section); }
                    if (section.Name == "AbsenceReasons") { _absenceReasonPermissions = parseSyncPermissionSection(section); }
                    if (section.Name == "AbsenceStatuses") { _absenceStatusPermissions = parseSyncPermissionSection(section); }
                    if (section.Name == "GradeLevels") { _gradeLevelPermissions = parseSyncPermissionSection(section); }
                    if (section.Name == "Schools") { _schoolPermissions = parseSyncPermissionSection(section); }
                    if (section.Name == "StudentGradePlacements") { _studentGradePlacementPermissions = parseSyncPermissionSection(section); }
                    if (section.Name == "Students") { _studentPermissions = parseSyncPermissionSection(section); }
                    if (section.Name == "ExpectedAttendance") { _expectedAttendancePermissions = parseSyncPermissionSection(section); }
                }
            }


            // Other settings
            List<XElement> otherSettingsSection = configFile.Elements("Other").ToList();
            // Loop through all "Other" sections (hopefully only one)
            foreach (XElement section in otherSettingsSection)
            {
                // Loop through each setting in this section
                foreach (XElement setting in section.Elements())
                {
                    if (setting.Name == "SchoolYearID")
                    {
                        schoolYearID = setting.Value;
                    }
                }
            }

            return new ConfigFile()
            {
                DatabaseConnectionString_Internal = connectionString_Internal,
                DatabaseConnectionString_SchoolLogic = connectionString_SL,
                SchoolYearName = schoolYearID,
                Loaded = true,
                AbsenceReasonPermissions = _absenceReasonPermissions,
                AbsenceStatusPermissions = _absenceStatusPermissions,
                AbsencePermissions = _absencePermissions,
                GradeLevelPermissions = _gradeLevelPermissions,
                SchoolPermissions = _schoolPermissions,
                StudentGradePlacementPermissions = _studentGradePlacementPermissions,
                StudentPermissions = _studentPermissions,
                ExpectedAttendancePermissions = _expectedAttendancePermissions

            };
        }
    }
}

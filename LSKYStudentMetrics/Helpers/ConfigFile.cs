using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSKYStudentMetrics.Helpers
{
    class ConfigFile
    {
        public string DatabaseConnectionString { get; set; }
        public string SchoolYearIdentifier { get; set; }

        public ConfigFile()
        {
            DatabaseConnectionString = "data source=HOSTNAME;initial catalog=DATABASE;user id=USERNAME;password=PASSWORD;Trusted_Connection=false";
            SchoolYearIdentifier = "Unknown";
        }
    }
}

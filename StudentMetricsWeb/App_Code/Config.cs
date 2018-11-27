using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Config
/// </summary>
public static class Config
{
    public static string dbConnectionString = ConfigurationManager.ConnectionStrings["InternalConnectionString"].ConnectionString ?? string.Empty;

}
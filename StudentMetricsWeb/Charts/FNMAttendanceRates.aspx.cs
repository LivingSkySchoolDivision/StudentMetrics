using System;
using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Charts;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Charts_FNMAttendanceRates : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        decimal targetAttendanceRate = (decimal)0.85;
        DateTime startDate = new DateTime(2018, 9, 4);
        DateTime endDate = DateTime.Today.AddHours(-1);

        FNMTargetAttendanceRateChart chart = new FNMTargetAttendanceRateChart()
        {
            StartDate = startDate,
            EndDate = endDate,
            TargetRate = targetAttendanceRate
        };

        SendImage(chart.Generate(Config.dbConnectionString));
    }

    private void SendImage(byte[] graphicBits)
    {
        Response.Clear();
        Response.ContentType = "image/png";
        Response.BinaryWrite(graphicBits);
        Response.End();
    }
}
using LSSDMetricsLibrary.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Graphs_AverageAttendanceRates : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        AverageAttendanceRateChart graph = new AverageAttendanceRateChart(Config.dbConnectionString, DateTime.Now.AddDays(-30), DateTime.Now, "Average Attendance Rates");

        SendImage(graph.DrawGraph(750, 1000));

    }

    private void SendImage(byte[] graphicBits)
    {
        Response.Clear();
        Response.ContentType = "image/png";
        Response.BinaryWrite(graphicBits);
        Response.End();
    }
}
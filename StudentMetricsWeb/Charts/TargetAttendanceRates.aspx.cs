using LSSDMetricsLibrary.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Graphs_TargetAttendanceRates : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        decimal targetAttendanceRate = (decimal)0.85;
        DateTime startDate = new DateTime(2018, 9, 4);
        DateTime endDate = DateTime.Today.AddHours(-1);
        FNMTargetAttendanceRateChart graph = new FNMTargetAttendanceRateChart(Config.dbConnectionString, startDate, endDate, targetAttendanceRate);
        SendImage(graph.DrawGraph());
    }

    private void SendImage(byte[] graphicBits)
    {
        Response.Clear();
        Response.ContentType = "image/png";
        Response.BinaryWrite(graphicBits);
        Response.End();
    }
}
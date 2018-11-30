using System;
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
        FNMTargetAttendanceRateChart graph = new FNMTargetAttendanceRateChart(Config.dbConnectionString, DateTime.Now.AddDays(-30), DateTime.Now, (decimal)0.85);
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
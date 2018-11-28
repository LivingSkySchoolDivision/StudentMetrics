﻿using LSSDMetricsLibrary.Graphs;
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
        AverageAttendanceRateGraph graph = new AverageAttendanceRateGraph(Config.dbConnectionString, DateTime.Now.AddDays(-30), DateTime.Now);

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
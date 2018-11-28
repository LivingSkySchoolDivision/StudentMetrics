using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Debug_Test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Get a student's attendance data

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        int iStudentID = 10876; // 600003512 Mark Emmett Dumais @ Leoville Central School
        DateTime startDate = new DateTime(2018, 09, 04);
        DateTime endDate = new DateTime(2018, 11, 27);


        InternalStudentRepository _studentRepo = new InternalStudentRepository(Config.dbConnectionString);
        Student student = _studentRepo.Get(iStudentID);

        Response.Write("<B>" + student.cStudentNumber + "</B><br>");

        InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(Config.dbConnectionString);

        Response.Write("<BR><BR>Loaded-Repo-Elapsed: " + stopwatch.Elapsed);

        StudentAttendanceRate sar = _attendanceRateRepo.GetForStudent(student.iStudentID);

        Response.Write("<BR><BR>Loaded-Student-Elapsed: " + stopwatch.Elapsed);

        Response.Write("<BR>Absences: " + sar.GetNumAbsences(startDate, endDate));
        Response.Write("<BR>Expected: " + sar.GetExpectedAttendance(startDate, endDate));
        Response.Write("<BR>Attendance Rate: " + sar.GetAttendanceRate(startDate, endDate));      



        stopwatch.Stop();
        Response.Write("<BR><BR>Elapsed: " + stopwatch.Elapsed);
        
        Response.Write("<BR>");

        InternalAbsenceRepository absenceRepo = new InternalAbsenceRepository(Config.dbConnectionString);
        foreach(Absence abs in absenceRepo.GetForStudent(iStudentID))
        {
            Response.Write("<BR>" + abs);
        }

    }
}
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
        int iSchoolID = 5850949; // Kerrobert
        //int iStudentID = 10876; // 600003512 Mark Emmett Dumais @ Leoville Central School
        DateTime startDate = new DateTime(2018, 09, 04);
        //DateTime endDate = new DateTime(2018, 11, 27);
        DateTime endDate = startDate;


        InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(Config.dbConnectionString);
        InternalStudentSchoolEnrolmentRepository _schoolStatusRepo = new InternalStudentSchoolEnrolmentRepository(Config.dbConnectionString);               
        InternalStudentRepository _studentRepo = new InternalStudentRepository(Config.dbConnectionString);
        InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(Config.dbConnectionString);

        School school = _schoolRepo.Get(iSchoolID);
        List<int> enrolledIDs = _schoolStatusRepo.GetStudentIDsEnrolledOn(startDate, true);
        List<Student> allStudents = _studentRepo.Get(_schoolStatusRepo.GetStudentIDsEnrolledOn(startDate, true));
        List<Student> schoolStudents = _studentRepo.Get(_schoolStatusRepo.GetStudentIDsEnrolledOn(startDate, iSchoolID, true));


        Response.Write("<br>Total IDs: " + enrolledIDs.Count);
        Response.Write("<br>Total Students: " + allStudents.Count);
        Response.Write("<br>School: " + school.Name);
        Response.Write("<br>School Students: " + schoolStudents.Count);

        List<decimal> schoolAttendanceRates = new List<decimal>();


        Response.Write("<table>");
        Response.Write("<tr><td>Student Number</td><td>IsFirstNations</td><td>Expected</td><td>Absences</td><td>Rate</td></tr>");
        foreach (Student s in schoolStudents)
        {
            StudentAttendanceRate sar = _attendanceRateRepo.GetForStudent(s.iStudentID);
            Response.Write("<tr>");
            Response.Write("<td>" + s.cStudentNumber + "</td>");
            Response.Write("<td>" + s.IsFirstNations + "</td>");
            Response.Write("<td>" + sar.GetExpectedAttendance(startDate, endDate) + "</td>");
            Response.Write("<td>" + sar.GetNumAbsences(startDate, endDate) + "</td>");
            Response.Write("<td>" + sar.GetAttendanceRate(startDate, endDate) + "</td>");
            Response.Write("</tr>");
        }
        Response.Write("</table>");        
        
        stopwatch.Stop();
        Response.Write("<BR><BR>Elapsed: " + stopwatch.Elapsed);
        
        Response.Write("<BR>");
    }
}
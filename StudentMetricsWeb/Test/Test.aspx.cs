using LSSDMetricsLibrary;
using LSSDMetricsLibrary.GraphDataPoints;
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
        string InternalConnectionString = Config.dbConnectionString;
        Dictionary<School, AverageAttendanceRateGraphDataPoint> _graphDataPoints = new Dictionary<School, AverageAttendanceRateGraphDataPoint>();
        DateTime startDate = new DateTime(2018, 09, 04);
        DateTime endDate = new DateTime(2018, 11, 28);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Response.Write("<BR>" + stopwatch.Elapsed + ": Stopwatch started");

        Response.Write("<BR>" + stopwatch.Elapsed + ": Init school repository");
        InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(InternalConnectionString);
        Response.Write("<BR>" + stopwatch.Elapsed + ": Init student reposistory");
        InternalStudentRepository _studentRepo = new InternalStudentRepository(InternalConnectionString);
        Response.Write("<BR>" + stopwatch.Elapsed + ": Init school status repository");
        InternalStudentSchoolEnrolmentRepository _schoolStatusRepo = new InternalStudentSchoolEnrolmentRepository(InternalConnectionString);
        Response.Write("<BR>" + stopwatch.Elapsed + ": Init attendance rate repository");
        InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(InternalConnectionString, startDate, endDate);
        Response.Write("<BR>" + stopwatch.Elapsed + ": Repository setup complete");
        // Generate some data points
        Response.Write("<BR>" + stopwatch.Elapsed + ": Start looping through schools");
        foreach (School school in _schoolRepo.GetAll())
        {
            Response.Write("<BR>" + stopwatch.Elapsed + ": SCHOOL: " + school.Name);

            // Load school students
            Response.Write("<BR>" + stopwatch.Elapsed + ": > Loading students");
            List<Student> schoolStudents = _studentRepo.Get(_schoolStatusRepo.GetStudentIDsEnrolledOn(startDate, endDate, school.iSchoolID, true));

            // Skip schools that have no students
            if (schoolStudents.Count == 0)
            {
                Response.Write("<BR>" + stopwatch.Elapsed + ": > Skipping, zero students");
                continue;
            }

            // Calculate each student's attendance rate for the given time period
            // Throw out rates that are -1, because they are invalid
            // Keep a running tally of all attendance rates, and of those from first nations students
            List<decimal> attendanceRatesAllStudents = new List<decimal>();
            List<decimal> attendanceRatesFNM = new List<decimal>();

            Response.Write("<BR>" + stopwatch.Elapsed + ": > Start attendance rate collection for all students");
            // This is where we see the tremendous lag
            foreach (Student s in schoolStudents)
            {
                StudentAttendanceRate sar = _attendanceRateRepo.GetForStudent(s.iStudentID, startDate, endDate);

                decimal attendanceRate = sar.GetAttendanceRate(startDate, endDate);
                if (attendanceRate != -1)
                {
                    attendanceRatesAllStudents.Add(attendanceRate);
                    if (s.IsFirstNations)
                    {
                        attendanceRatesFNM.Add(attendanceRate);
                    }
                }
            }

            if (attendanceRatesAllStudents.Count == 0)
            {
                Response.Write("<BR>" + stopwatch.Elapsed + ": > No students with attendance rates, skipping to next school");
                continue;
            }

            Response.Write("<BR>" + stopwatch.Elapsed + ": > Begin calculating average attendance rate for the school");
            // Average them all together and build the data point object for the graph
            _graphDataPoints.Add(school, new AverageAttendanceRateGraphDataPoint()
            {
                AttendanceRate = (attendanceRatesAllStudents.Count > 0) ? attendanceRatesAllStudents.Average() : -1,
                AttendanceRate_FNM = (attendanceRatesFNM.Count > 0) ? attendanceRatesFNM.Average() : -1
            });
        }
        Response.Write("<BR>" + stopwatch.Elapsed + ": Finished creating data points for all schools");        
        stopwatch.Stop();
        Response.Write("<BR><BR>Final stopwatch time: " + stopwatch.Elapsed);

        Response.Write("<HR><b>Graph Data</b>");
        foreach(School school in _graphDataPoints.Keys)
        {
            Response.Write("<BR>&nbsp;<b>" + school.Name + "</b>: " + _graphDataPoints[school].FriendlyAttendanceRate + "/" + _graphDataPoints[school].FriendlyAttendanceRate_FNM);
        }        

        Response.Write("<BR>");
    }
}
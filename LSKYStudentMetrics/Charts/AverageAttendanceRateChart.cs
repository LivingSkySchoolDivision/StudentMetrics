using LSSDMetricsLibrary;
using LSSDMetricsLibrary.Repositories.Internal;
using LSSDMetricsLibrary.GraphDataPoints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using LSSDMetricsLibrary.Extensions;

namespace LSSDMetricsLibrary.Charts
{

    public class AverageAttendanceRateChart
    {
        Dictionary<School, AverageAttendanceRateGraphDataPoint> _graphDataPoints = new Dictionary<School, AverageAttendanceRateGraphDataPoint>();
        private string chartTitle = string.Empty;
        private string chartSubTitle = string.Empty;

        public AverageAttendanceRateChart(string InternalConnectionString, DateTime startDate, DateTime endDate, string ChartTitle)
        {
            chartTitle = ChartTitle;
            chartSubTitle = startDate.ToShortDateString() + " to " + endDate.ToShortDateString();
            // Load all schools
            InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(InternalConnectionString);
            InternalStudentRepository _studentRepo = new InternalStudentRepository(InternalConnectionString);
            InternalStudentSchoolEnrolmentRepository _schoolStatusRepo = new InternalStudentSchoolEnrolmentRepository(InternalConnectionString);
            InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(InternalConnectionString, startDate, endDate);

            // Generate some data points
            foreach (School school in _schoolRepo.GetAll())
            {
                // Load school students
                List<Student> schoolStudents = _studentRepo.Get(_schoolStatusRepo.GetStudentIDsEnrolledOn(startDate, endDate, school.iSchoolID, true));

                // Skip schools that have no students
                if (schoolStudents.Count == 0)
                {
                    continue;
                }

                // Calculate each student's attendance rate for the given time period
                // Throw out rates that are -1, because they are invalid
                // Keep a running tally of all attendance rates, and of those from first nations students
                List<decimal> attendanceRatesAllStudents = new List<decimal>();
                List<decimal> attendanceRatesFNM = new List<decimal>();
                
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
                    continue;
                }

                // Average them all together and build the data point object for the graph
                _graphDataPoints.Add(school, new AverageAttendanceRateGraphDataPoint()
                {
                    AttendanceRate = (attendanceRatesAllStudents.Count > 0) ? attendanceRatesAllStudents.Average() : -1,
                    AttendanceRate_FNM = (attendanceRatesFNM.Count > 0) ? attendanceRatesFNM.Average() : -1
                });
            }
        }

        public byte[] DrawGraph(int width, int height)
        {
            MemoryStream returnedBytes = new MemoryStream();
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            Font font_title = new Font("Arial", 16, FontStyle.Bold);
            Font font_subtitle = new Font("Arial", 12, FontStyle.Bold);
            Font font_Label = new Font("Arial", 12, FontStyle.Bold);
            Brush labelBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush titleBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush barBrush = new SolidBrush(Color.FromArgb(255, 49, 55, 115));
            Brush barValueBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            Font barValueFont = new Font("Arial", 10, FontStyle.Bold);
            Pen graphAxisPen = new Pen(new SolidBrush(Color.FromArgb(128, 0, 0, 0)));
            Pen feintLinePen = new Pen(new SolidBrush(Color.FromArgb(64, 0, 0, 0)));

            // Fiddly variables
            int barPadding = 4;
            int labelPadding_Left = 0;
            int spacing_labelAndGraph = 10;
            int titleSpace = 50;

            // variables that will fiddle themselves out later
            int dataPointHeight = 0;
            float largestLabelHeight = 0;
            float largestLabelWidth = 0;
            int thisBarStartY = titleSpace;
            float graphStartX = 0;
            int totalUsedImageHight = height;
            int totalUsedImageWidth = width;
            float barHeight = 10;
            int barMaxWidth = 0;

            // Set a background colour
            graphics.Clear(Color.White);
            
            // Calculate the label width max
            foreach (School school in this._graphDataPoints.Keys)
            {
                SizeF labelSize = graphics.MeasureString(school.ShortName, font_Label);
                if (labelSize.Width > largestLabelWidth)
                {
                    largestLabelWidth = labelSize.Width;
                }

                if (labelSize.Height > largestLabelHeight)
                {
                    largestLabelHeight = labelSize.Height;
                }                
            }
            dataPointHeight = (int)largestLabelHeight + barPadding;
            barHeight = dataPointHeight * (float)0.75;
            
            graphStartX = largestLabelWidth + spacing_labelAndGraph;
            barMaxWidth = width - (int)graphStartX;

            // Chart title
            graphics.DrawString(chartTitle, font_title, titleBrush, width/2, 0, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
            graphics.DrawString(chartSubTitle, font_subtitle, titleBrush, width/2, 25, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
            

            // Chart data
            foreach (School school in this._graphDataPoints.Keys.OrderBy(x => _graphDataPoints[x]))
            {
                float thisBarVerticalMiddle = thisBarStartY + (dataPointHeight / 2);
                float thisBarWidth = barMaxWidth * (float)(_graphDataPoints[school].AttendanceRate); // This should be the actual width of the bar
                
                // Draw a separating line
                //graphics.DrawLine(feintLinePen, new Point(0, (int)thisBarVerticalMiddle), new Point(width, (int)thisBarVerticalMiddle));
                //graphics.DrawLine(feintLinePen, new Point(0, (int)thisBarStartY), new Point(width, (int)thisBarStartY));
                //graphics.DrawLine(feintLinePen, new Point(0, (int)thisBarStartY), new Point((int)graphStartX, (int)thisBarStartY));

                // Draw the label
                //graphics.DrawString(school.ShortName, font_Label, labelBrush, labelPadding_Left, thisBarVerticalMiddle, new StringFormat() { LineAlignment = StringAlignment.Center });
                graphics.DrawString(school.ShortName, font_Label, labelBrush, graphStartX - labelPadding_Left, thisBarVerticalMiddle, new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far  });

                // Draw the value bar                
                graphics.FillRectangle(barBrush, graphStartX, thisBarVerticalMiddle - (barHeight/2), thisBarWidth, barHeight);

                graphics.DrawString(_graphDataPoints[school].FriendlyAttendanceRate, barValueFont, barValueBrush, graphStartX, thisBarVerticalMiddle, new StringFormat() { LineAlignment = StringAlignment.Center });

                thisBarStartY += dataPointHeight;
            }
            totalUsedImageHight = thisBarStartY;

            // Add the line that starts the graph
            graphics.DrawLine(graphAxisPen, new Point((int)graphStartX, titleSpace), new Point((int)graphStartX, thisBarStartY));

            // Resize the bitmap
            bitmap.Crop(totalUsedImageWidth, totalUsedImageHight).Save(returnedBytes, ImageFormat.Png);
            return returnedBytes.ToArray();
        }

    }
}

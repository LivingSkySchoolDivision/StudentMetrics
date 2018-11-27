using LSKYStudentMetrics;
using LSKYStudentMetrics.Repositories.Internal;
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

namespace LSSDMetricsLibrary.Graphs
{

    public class AverageAttendanceRateGraph
    {
        Dictionary<School, AverageAttendanceRateGraphDataPoint> _graphDataPoints = new Dictionary<School, AverageAttendanceRateGraphDataPoint>();
        
        public AverageAttendanceRateGraph(string InternalConnectionString, DateTime startDate, DateTime endDate)
        {
            // Load all schools
            InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(InternalConnectionString);
            InternalStudentRepository _studentRepo = new InternalStudentRepository(InternalConnectionString);


            // Generate some data points
            foreach (School school in _schoolRepo.GetAll())
            {
                _graphDataPoints.Add(school, new AverageAttendanceRateGraphDataPoint() { AttendanceRate = (float)0.8 });

                // Load all students that attended this school during the specified time

                // I want to end up with an actual attendance rate VALUE for each student
                // We COULD cut a corner here and add up total expected blocks for the whole school and compare that to the total 
                // absences for the whole school, but we want to use the individual student attendance rates in a future graph

                // StudentAttendanceRate(StudentSchedule, StudentAbsences)
                //  StudentAttendanceRate.GetRateFor(DateTime from, DateTime to)

                //List<Student> schoolStudents = _studentRepo.GetForSchool(school, startDate, endDate);
            }
        }

        public byte[] DrawGraph(int width, int height)
        {
            MemoryStream returnedBytes = new MemoryStream();
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            Font font_Label = new Font("Arial", 12, FontStyle.Bold);
            Brush labelBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush barBrush = new SolidBrush(Color.FromArgb(255, 49, 55, 115));
            Brush barValueBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            Font barValueFont = new Font("Arial", 10, FontStyle.Bold);
            Pen graphAxisPen = new Pen(new SolidBrush(Color.FromArgb(128, 0, 0, 0)));
            Pen feintLinePen = new Pen(new SolidBrush(Color.FromArgb(64, 0, 0, 0)));

            // Fiddly variables
            int barPadding = 4;
            int labelPadding_Left = 0;
            int spacing_labelAndGraph = 10;   

            // variables that will fiddle themselves out later
            int dataPointHeight = 0;
            float largestLabelHeight = 0;
            float largestLabelWidth = 0;
            int thisBarStartY = 0;
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
            thisBarStartY = 0;
            foreach (School school in this._graphDataPoints.Keys.OrderBy(x => _graphDataPoints[x]))
            {
                float thisBarVerticalMiddle = thisBarStartY + (dataPointHeight / 2);
                float thisBarWidth = barMaxWidth * (float)(_graphDataPoints[school].AttendanceRate); // This should be the actual width of the bar

                // Draw a separating line
                //graphics.DrawLine(feintLinePen, new Point(0, (int)thisBarVerticalMiddle), new Point(width, (int)thisBarVerticalMiddle));
                //graphics.DrawLine(feintLinePen, new Point(0, (int)thisBarStartY), new Point(width, (int)thisBarStartY));
                graphics.DrawLine(feintLinePen, new Point(0, (int)thisBarStartY), new Point((int)graphStartX, (int)thisBarStartY));

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
            graphics.DrawLine(graphAxisPen, new Point((int)graphStartX, 0), new Point((int)graphStartX, thisBarStartY));

            // Resize the bitmap
            bitmap.Crop(totalUsedImageWidth, totalUsedImageHight).Save(returnedBytes, ImageFormat.Png);
            return returnedBytes.ToArray();
        }

    }
}

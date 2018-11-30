using LSSDMetricsLibrary.Model.ChartParts;
using LSSDMetricsLibrary.Repositories.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Charts
{
    public class TargetAttendanceRateChart
    {
        public string Title = string.Empty;
        public string SubTitle = string.Empty;
        public int Height = 700;
        public int Width = 750;
        public bool ShowBarValuesOnEndOfBar = false;
        public bool SortByValue = true;
        public bool IncludeFirstNationsRate = true;
        
        List<BarChartDataSeries> _chartData = new List<BarChartDataSeries>();
        Dictionary<int, string> _legend = new Dictionary<int, string>();

        List<Brush> barColours = new List<Brush>()
        {
            new SolidBrush(Color.FromArgb(255, 8, 34, 89)),
            new SolidBrush(Color.FromArgb(255, 1, 77, 81)),
            new SolidBrush(Color.FromArgb(255, 133, 61, 0)),
            new SolidBrush(Color.FromArgb(255, 133, 88, 0)),
        };
        int barColourBrushCounter = 0;
        private Brush getNextBarColour()
        {
            if (barColours.Count == 0) { throw new Exception("Bar colour list empty, can't get next colour!"); }
            Brush returnMe = barColours[barColourBrushCounter];
            barColourBrushCounter++;
            if (barColourBrushCounter >= barColours.Count)
            {
                barColourBrushCounter = 0;
            }
            return returnMe;
        }
        
        public TargetAttendanceRateChart(string InternalConnectionString, DateTime startDate, DateTime endDate, decimal targetRate)
        {
            Title = "% Students with at least " + ((decimal)targetRate*100).ToString("0") + "% Attendance Rate";
            SubTitle = startDate.ToShortDateString() + " to " + endDate.ToShortDateString();
            
            // Load all schools
            InternalSchoolRepository _schoolRepo = new InternalSchoolRepository(InternalConnectionString);
            InternalStudentRepository _studentRepo = new InternalStudentRepository(InternalConnectionString);
            InternalStudentSchoolEnrolmentRepository _schoolStatusRepo = new InternalStudentSchoolEnrolmentRepository(InternalConnectionString);
            InternalStudentAttendanceRateRepository _attendanceRateRepo = new InternalStudentAttendanceRateRepository(InternalConnectionString, startDate, endDate);

            _chartData = new List<BarChartDataSeries>();

            // Set up the legend
            _legend = new Dictionary<int, string>()
            {
                {0,"Total" },
                {1,"First Nations" }
            };
            
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
                        if ((this.IncludeFirstNationsRate) &&(s.IsFirstNations))
                        {
                            attendanceRatesFNM.Add(attendanceRate);
                        }
                    }
                }

                if (attendanceRatesAllStudents.Count == 0)
                {
                    continue;
                }

                BarChartDataSeries schoolGraphData = new BarChartDataSeries()
                {
                    Label = school.ShortName
                };
                
                if (attendanceRatesAllStudents.Count > 0)
                {
                    decimal totalAttendanceRate = (decimal)((decimal)attendanceRatesAllStudents.Count(x => x >= targetRate) / (decimal)attendanceRatesAllStudents.Count());
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = totalAttendanceRate,
                        Label = (totalAttendanceRate * 100).ToString("0.##") + "%",
                        ID = 0
                    });
                }

                if (attendanceRatesFNM.Count > 0)
                {
                    decimal fnmAttendanceRate = (decimal)((decimal)attendanceRatesFNM.Count(x => x >= targetRate) / (decimal)attendanceRatesFNM.Count());
                    schoolGraphData.DataPoints.Add(new BarChartPercentBar()
                    {
                        Value = fnmAttendanceRate,
                        Label = (fnmAttendanceRate * 100).ToString("0.##") + "%",
                        ID = 1
                    });
                }

                if (schoolGraphData.DataPoints.Count > 0)
                {
                    _chartData.Add(schoolGraphData);
                }
            }
        }

        public byte[] DrawGraph()
        {            
            Font font_title = new Font("Arial", 16, FontStyle.Bold);
            Font font_subtitle = new Font("Arial", 12, FontStyle.Bold);
            Font font_Label = new Font("Arial", 10, FontStyle.Bold);
            Font font_barValue = new Font("Arial", 10, FontStyle.Bold);
            Font font_Legend = new Font("Arial", 8, FontStyle.Bold);
            Brush brush_Label = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush brush_Title = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush brush_BarValue = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            Brush brush_BarValueAlt = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Pen pen_GraphAxis = new Pen(new SolidBrush(Color.FromArgb(64, 0, 0, 0)));            
            Color graphBackgroundColor = Color.White;

            int titleAfterPadding = 5; // How much padding should we add after the title
            int barPadding = 0; // Spacing between each bar in a series
            int barRightEdgePadding = 5; // Spacing to leave on the right edge of the graphic
            int seriesPadding = 5; // Spacing between each series
            float barHeight = 20; // Height in pixels that each bar will be
            int labelPadding = 0; // Padding between the label and the start of the bars
            int legendAfterPadding = 10; // Padding after the legend, if a legend is displayed
            int legendBoxSize = (int)barHeight; // How big (in pixels) to make the legend colour box
            int legendItemPadding = 4; // Padding between legend items
            int legendTextSpacing = 2; // Spacing between the legend colour box and the text

            // Variables that will sort themselves out as the graph gets generated. 
            // Probably best to not touch these
            float actualTitleAreaSpace = 0;
            float largestLabelHeight = 0;
            float largestLabelWidth = 0;
            float barMaxWidth = 0;
            
            // Set up the bitmap and graphics objects
            Bitmap bitmap = new Bitmap(Width, Height);
            Graphics graphics = Graphics.FromImage(bitmap);


            // Calculate the label width max
            // Calculate the total hight we're going to need
            int detectedNumberOfBars = 0;
            int maxBarsPerSeries = 0;
            foreach (BarChartDataSeries label in _chartData)
            {                
                SizeF labelSize = graphics.MeasureString(label.Label, font_Label);
                // Calculate max label width
                if (labelSize.Width > largestLabelWidth)
                {
                    largestLabelWidth = labelSize.Width;
                }
                // Calculate max label height
                if (labelSize.Height > largestLabelHeight)
                {
                    largestLabelHeight = labelSize.Height;
                }                

                if (label.DataPoints.Count > maxBarsPerSeries)
                {
                    maxBarsPerSeries = label.DataPoints.Count;
                }

                // Add to height tally
                detectedNumberOfBars += label.DataPoints.Count();
            }
            
            // Set up bar colours
            Dictionary<int, Brush> barColoursByID = new Dictionary<int, Brush>();
            foreach(int id in _legend.Keys)
            {
                barColoursByID.Add(id, getNextBarColour());
            }

            // Calculate the space we have for bars
            barMaxWidth = Width - largestLabelWidth - labelPadding - barRightEdgePadding;

            // Calculate how much space we need for the title section
            SizeF titleSize = graphics.MeasureString(Title, font_title);
            SizeF subTitleSize = graphics.MeasureString(SubTitle, font_subtitle);
            actualTitleAreaSpace = titleSize.Height + subTitleSize.Height + titleAfterPadding;
            float titleY = 0;
            float subTitleY = titleY + titleSize.Height;

            // Calculate how much space we need for the legend
            float totalLegendHeight = 0;
            if (maxBarsPerSeries > 1)
            {
                totalLegendHeight = legendAfterPadding + (_legend.Count * (legendBoxSize + legendItemPadding));
            }
                        
            // Calculate how much space we'll need for all the bars            
            float totalSeriesHeight =  (detectedNumberOfBars * (barHeight + barPadding)) + (seriesPadding * _chartData.Count);

            // So now, the total height required will be title area plus total series hight
            float minHeightRequired = actualTitleAreaSpace + totalLegendHeight + totalSeriesHeight;

            // Resize the bitmap if we need to
            if (minHeightRequired > Height)
            {
                Height = (int)minHeightRequired;
                bitmap = new Bitmap(Width, Height);
                graphics = Graphics.FromImage(bitmap);
            }

            
            // Start to draw the chart

            // Set a background colour
            graphics.Clear(graphBackgroundColor);

            // Title Area
            
            graphics.DrawString(Title, font_title, brush_Title, Width / 2, titleY, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
            graphics.DrawString(SubTitle, font_subtitle, brush_Title, Width / 2, subTitleY, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });

            // Legend
            if (maxBarsPerSeries > 1)
            {
                // Calculate maximum legend text width (so we can center everything
                float maxLegendItemWidth = 0;
                foreach (KeyValuePair<int, string> legendItem in _legend)
                {
                    SizeF legendItemSize = graphics.MeasureString(legendItem.Value, font_Legend);
                    if (legendItemSize.Width > maxLegendItemWidth)
                    {
                        maxLegendItemWidth = legendItemSize.Width;
                    }
                }

                float legendWidth = maxLegendItemWidth + legendTextSpacing + legendBoxSize;
                float legendYPosition = actualTitleAreaSpace;
                float legendBoxX = (Width / 2) - (legendWidth / 2);
                float legendTextX = legendBoxX + legendBoxSize + legendTextSpacing;
                foreach (KeyValuePair<int, string> legendItem in _legend)
                {
                    // Box
                    graphics.FillRectangle(barColoursByID[legendItem.Key], legendBoxX, legendYPosition, legendBoxSize, legendBoxSize);
                    
                    // Label                    
                    float textY = legendYPosition + (legendBoxSize / 2);
                    graphics.DrawString(legendItem.Value, font_Legend, brush_Label, legendTextX, textY, new StringFormat() { LineAlignment = StringAlignment.Center });
                    legendYPosition += legendBoxSize + legendItemPadding;
                }
            }

            // Chart data                       
            float totalSpaceAboveData = actualTitleAreaSpace + totalLegendHeight;
            float currentYPosition = totalSpaceAboveData;
            float graphStartX = largestLabelWidth;
            foreach (BarChartDataSeries series in this.SortByValue ? _chartData.OrderByDescending(x=>x.DataPoints.Where(y => y.ID == 0).Max(y => y.Value)).ToList() : _chartData)
            {
                float thisSeriesY = currentYPosition;

                // Center the label vertically
                float thisLabelY = thisSeriesY + (((barHeight + barPadding) * series.DataPoints.Count)/2) - barPadding;

                // Draw the label                
                graphics.DrawString(series.Label, font_Label, brush_Label, graphStartX, thisLabelY, new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far });
                
                // Draw each bar associated with the series
                foreach (BarChartPercentBar bar in series.DataPoints)
                {
                    float thisBarY = currentYPosition;
                    float thisBarMiddleY = thisBarY + (barHeight / 2);

                    // Get the colour for this bar
                    Brush thisBarBrush = barColoursByID[bar.ID];

                    // Calculate this bar's width
                    float thisBarWidth = barMaxWidth * (float)bar.Value;

                    // Draw the bar itself
                    graphics.FillRectangle(thisBarBrush, graphStartX + labelPadding, thisBarY, thisBarWidth, barHeight);
                    
                    // Add the value text
                    // Check if we need to use an alt colour for the text because the bar is too small
                    Brush thisBarValueBrush = brush_BarValue;
                    SizeF barvalueTextSize = graphics.MeasureString(bar.Label, font_barValue);
                    if (thisBarWidth < barvalueTextSize.Width)
                    {
                        thisBarValueBrush = brush_BarValueAlt;
                    }

                    if (this.ShowBarValuesOnEndOfBar)
                    {
                        float textX = graphStartX + thisBarWidth;
                        if (textX < graphStartX + barvalueTextSize.Width)
                        {
                            textX = graphStartX + barvalueTextSize.Width;
                        }
                        graphics.DrawString(bar.Label, font_barValue, thisBarValueBrush, textX, thisBarMiddleY, new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far });
                    }
                    else
                    {
                        graphics.DrawString(bar.Label, font_barValue, thisBarValueBrush, graphStartX, thisBarMiddleY, new StringFormat() { LineAlignment = StringAlignment.Center });                        
                    }                    
                    
                    currentYPosition += barHeight + barPadding;
                }

                currentYPosition += seriesPadding;
            }
            
            // Draw a line separating the labels from the bars
            graphics.DrawLine(pen_GraphAxis, graphStartX, totalSpaceAboveData, graphStartX, currentYPosition);
            
            // Resize the bitmap
            MemoryStream returnedBytes = new MemoryStream();
            bitmap.Save(returnedBytes, ImageFormat.Png);
            return returnedBytes.ToArray();
        }
    }
}

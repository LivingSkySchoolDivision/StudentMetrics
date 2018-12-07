using LSSDMetricsLibrary.Model.ChartParts;
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
    class LineChartGenerator
    {
        public string Title = string.Empty;
        public string SubTitle = string.Empty;
        public string SubSubTitle = string.Empty;
        public int Width = 1000;
        public int Height = 750;
        public bool ShowValuesInChart = false;
        public bool ShowYGridLines = true;
        public bool ShowXGridLines = false;

        protected List<string> Labels { get; set; }
        protected List<LineChartLine> Lines { get; set; }

        public LineChartGenerator()
        {
            this.Labels = new List<string>();
            this.Lines = new List<LineChartLine>();
        }

        protected List<Pen> LineColours = new List<Pen>()
        {
            new Pen(Color.FromArgb(255, 8, 42, 167)),
            new Pen(Color.FromArgb(255, 246, 86, 0)),
            new Pen(Color.FromArgb(255, 0, 163, 114)),
            new Pen(Color.FromArgb(255, 234, 0, 32)),
        };

        int lineColourCounter = 0;
        private Pen getNextColour()
        {
            // If no colors are present (because the consumer of this class overrode the bar colours list with an empty one),
            // Default to black
            if (LineColours.Count == 0) { return new Pen(Color.FromArgb(255, 0, 0, 0)); }
            Pen returnMe = LineColours[lineColourCounter];
            lineColourCounter++;
            if (lineColourCounter >= LineColours.Count)
            {
                lineColourCounter = 0;
            }
            return returnMe;
        }

        private Dictionary<string, Pen> _lineColourMappings = new Dictionary<string, Pen>();
        private Pen getLineColour(string id)
        {
            if (!_lineColourMappings.ContainsKey(id))
            {
                _lineColourMappings.Add(id, getNextColour());
            }

            return _lineColourMappings[id];
        }
        public byte[] DrawGraph()
        {
            if (this.Lines.Count == 0)
            {
                throw new NoGraphDataException("No chart data, can't create chart graph.");
            }

            if (this.Labels.Count == 0)
            {
                throw new NoGraphDataException("Missing list of chart labels!");
            }

            Font font_title = new Font("Arial", 16, FontStyle.Bold);
            Font font_subtitle = new Font("Arial", 12, FontStyle.Bold);
            Font font_subsubtitle = new Font("Arial", 10, FontStyle.Bold);
            Font font_xAxisLabels = new Font("Arial", 10, FontStyle.Bold);
            Font font_yAxisLabels = new Font("Arial", 10, FontStyle.Bold);
            Font font_Legend = new Font("Arial", 8, FontStyle.Bold);
            Font font_DataPointValue = new Font("Arial", 8, FontStyle.Bold);

            Brush brush_LegendLabels = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush brush_xAxisLabels = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush brush_yAxisLabels = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush brush_Title = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            Brush brush_DataPointValue = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            Pen pen_GraphAxis = new Pen(new SolidBrush(Color.FromArgb(255, 0, 0, 0)), 2);
            Pen pen_AxisPips = new Pen(new SolidBrush(Color.FromArgb(255, 0, 0, 0)), 1);
            Pen pen_GridLines = new Pen(new SolidBrush(Color.FromArgb(64, 0, 0, 0)), 1);
            Color graphBackgroundColor = Color.White;

            int titleAfterPadding = 5; // How much padding should we add after the title                                                            
            int legendAfterPadding = 10; // Padding after the legend, if a legend is displayed
            int legendBoxSize = 20; // How big (in pixels) to make the legend colour box
            int legendItemPadding = 4; // Padding between legend items
            int legendTextSpacing = 2; // Spacing between the legend colour box and the text       
            int chartRightPadding = 15; // How much whitespace should be at the right edge of the image (should probably be at least 15)
            int lineWidth = 2; // Width of the data lines
            int yAxisLabelZoneWidth = 40;
            int xAxisLabelZoneHeight = 30;
            int yAxisPipSize = 5;
            int xAxisPipSize = 5;
            int dataPointPipSize = 5;
            decimal yAxisInterval = (decimal)0.1; // How often are the pips and labels on the y axis (In percent)?

            // Variables that will sort themselves out as the graph gets generated. 
            // Probably best to not touch these
            float actualTitleAreaSpace = 0;
            float largestLabelHeight = 0;
            float largestLabelWidth = 0;

            // Set up the bitmap and graphics objects
            Bitmap bitmap = new Bitmap(Width, Height);
            Graphics graphics = Graphics.FromImage(bitmap);

            // Calculate how much space we need for the title section            
            actualTitleAreaSpace = 0;

            float titleY = 0;
            float subTitleY = 0;
            float subSubTitleY = 0;

            if (!string.IsNullOrEmpty(this.Title))
            {
                SizeF titleSize = graphics.MeasureString(Title, font_title);
                actualTitleAreaSpace += titleSize.Height;
            }

            if (!string.IsNullOrEmpty(this.SubTitle))
            {
                SizeF subTitleSize = graphics.MeasureString(SubTitle, font_subtitle);
                actualTitleAreaSpace += subTitleSize.Height;
                subTitleY = titleY + subTitleSize.Height;
            }

            if (!string.IsNullOrEmpty(this.SubSubTitle))
            {
                SizeF subSubTitleSize = graphics.MeasureString(SubSubTitle, font_subsubtitle);
                actualTitleAreaSpace += subSubTitleSize.Height;
                subSubTitleY = subTitleY + subSubTitleSize.Height;
            }

            actualTitleAreaSpace += titleAfterPadding;


            // Calculate how much space we need for the legend
            float totalLegendHeight = 0;
            if (this.Lines.Count > 1)
            {
                totalLegendHeight = legendAfterPadding + (Lines.Count * (legendBoxSize + legendItemPadding));
            }

            // So now, the total height required will be title area plus total series hight
            float chartAreaStartY = actualTitleAreaSpace + totalLegendHeight;

            float chartLeftEdge = yAxisLabelZoneWidth;
            float chartRightEdge = Width - chartRightPadding;
            float chartTopEdge = chartAreaStartY;
            float chartBottomEdge = Height - xAxisLabelZoneHeight;

            float chartWidth = chartRightEdge - chartLeftEdge;
            float chartHeight = chartBottomEdge - chartTopEdge;

            // Now, we can calculate the x coordinates of each data point
            float pointSpacing = chartWidth / (float)(this.Labels.Count - 1);
            Dictionary<string, float> dataPointXes = new Dictionary<string, float>();
            float calcCounter_labelSpacing = chartLeftEdge;
            foreach (string label in this.Labels)
            {
                dataPointXes.Add(label, calcCounter_labelSpacing);
                calcCounter_labelSpacing += pointSpacing;
            }

            // Set a background colour
            graphics.Clear(graphBackgroundColor);

            // Draw titles
            if (!string.IsNullOrEmpty(this.Title))
            {
                graphics.DrawString(Title, font_title, brush_Title, Width / 2, titleY, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
            }

            if (!string.IsNullOrEmpty(this.SubTitle))
            {
                graphics.DrawString(SubTitle, font_subtitle, brush_Title, Width / 2, subTitleY, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
            }

            if (!string.IsNullOrEmpty(this.SubSubTitle))
            {
                graphics.DrawString(SubSubTitle, font_subsubtitle, brush_Title, Width / 2, subSubTitleY, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
            }

            // Legend
            if (this.Lines.Count > 1)
            {
                // Calculate maximum legend text width (so we can center everything
                float maxLegendItemWidth = 0;
                foreach (string lineLabel in Lines.Select(x => x.Label))
                {
                    SizeF legendItemSize = graphics.MeasureString(lineLabel, font_Legend);
                    if (legendItemSize.Width > maxLegendItemWidth)
                    {
                        maxLegendItemWidth = legendItemSize.Width;
                    }
                }

                float legendWidth = maxLegendItemWidth + legendTextSpacing + legendBoxSize;
                float legendYPosition = actualTitleAreaSpace;
                float legendBoxX = (Width / 2) - (legendWidth / 2);
                float legendTextX = legendBoxX + legendBoxSize + legendTextSpacing;
                foreach (string lineLabel in Lines.Select(x => x.Label))
                {
                    Brush thisItemColour = new SolidBrush(getLineColour(lineLabel).Color);

                    // Box
                    graphics.FillRectangle(thisItemColour, legendBoxX, legendYPosition, legendBoxSize, legendBoxSize);

                    // Label                    
                    float textY = legendYPosition + (legendBoxSize / 2);
                    graphics.DrawString(lineLabel, font_Legend, brush_LegendLabels, legendTextX, textY, new StringFormat() { LineAlignment = StringAlignment.Center });
                    legendYPosition += legendBoxSize + legendItemPadding;
                }
            }

            // Draw the y axis
            graphics.DrawLine(pen_GraphAxis, chartLeftEdge, chartTopEdge, chartLeftEdge, chartBottomEdge);

            for (decimal pp = 0; pp < 1; pp += yAxisInterval)
            {
                float thisPipY = chartTopEdge + (chartHeight * (float)pp);
                graphics.DrawLine(pen_AxisPips, chartLeftEdge, thisPipY, chartLeftEdge + yAxisPipSize, thisPipY);
                graphics.DrawString(((1 - pp) * 100).ToString("0") + "%", font_yAxisLabels, brush_yAxisLabels, chartLeftEdge, thisPipY, new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far });
                if (this.ShowYGridLines)
                {
                    graphics.DrawLine(pen_GridLines, chartLeftEdge, thisPipY, chartRightEdge, thisPipY);
                }
            }

            // Draw the x axis   
            graphics.DrawLine(pen_GraphAxis, chartLeftEdge, chartBottomEdge, chartRightEdge, chartBottomEdge);
            foreach (string label in this.Labels)
            {
                graphics.DrawLine(pen_AxisPips, dataPointXes[label], chartBottomEdge, dataPointXes[label], chartBottomEdge - xAxisPipSize);
                graphics.DrawString(label, font_xAxisLabels, brush_xAxisLabels, dataPointXes[label], chartBottomEdge, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
                if (this.ShowXGridLines)
                {
                    graphics.DrawLine(pen_GridLines, dataPointXes[label], chartBottomEdge, dataPointXes[label], chartTopEdge);
                }
            }
            
            // Draw Lines
            foreach (LineChartLine line in this.Lines)
            {
                Pen thisLinePen = getLineColour(line.Label);
                thisLinePen.Width = lineWidth;
                Brush thisLineBrush = new SolidBrush(thisLinePen.Color);
                List<PointF> linePoints = new List<PointF>();

                // Calulate points
                foreach (string label in this.Labels)
                {
                    if (line.LineDataPoints.ContainsKey(label))
                    {
                        float thisX = (int)dataPointXes[label];
                        float thisY = (int)(chartBottomEdge - (chartHeight * (float)line.LineDataPoints[label]));
                        PointF thisPoint = new PointF(thisX,thisY);
                        linePoints.Add(thisPoint);
                    }
                }

                // Draw lines
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.DrawLines(thisLinePen, linePoints.ToArray());
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                //graphics.DrawCurve(thisLinePen, linePoints.ToArray());

                // Draw pips and text
                foreach (string label in this.Labels)
                {
                    if (line.LineDataPoints.ContainsKey(label))
                    {
                        float thisX = (int)dataPointXes[label];
                        float thisY = (int)(chartBottomEdge - (chartHeight * (float)line.LineDataPoints[label]));

                        if (this.ShowValuesInChart)
                        {
                            graphics.DrawString((line.LineDataPoints[label] * 100).ToString("0.##") + "%", font_DataPointValue, brush_DataPointValue, thisX, thisY + dataPointPipSize, new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });
                        }
                        graphics.FillRectangle(thisLineBrush, thisX - (dataPointPipSize / 2), thisY - (dataPointPipSize / 2), dataPointPipSize, dataPointPipSize);                        
                    }
                }
            }
                        

            // Return the bitmap as a PNG
            MemoryStream returnedBytes = new MemoryStream();
            bitmap.Save(returnedBytes, ImageFormat.Png);
            return returnedBytes.ToArray();
        }
    }
}

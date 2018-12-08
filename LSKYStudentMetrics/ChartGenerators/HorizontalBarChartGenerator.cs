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
    class HorizontalBarChartGenerator
    {
        public string Title = string.Empty;
        public string SubTitle = string.Empty;
        public string SubSubTitle = string.Empty;
        public int Height = 200;
        public int Width = 750;
        public bool ShowBarValuesOnEndOfBar = true;

        protected List<BarChartDataSeries> ChartData = new List<BarChartDataSeries>();
        protected Dictionary<int, string> Legend = new Dictionary<int, string>();

        public HorizontalBarChartGenerator() { }

        public HorizontalBarChartGenerator(List<BarChartDataSeries> ChartData, Dictionary<int, string> Legend)
        {
            this.ChartData = ChartData;
            this.Legend = Legend;
        }

        protected List<Brush> BarColours = new List<Brush>()
        {
            new SolidBrush(Color.FromArgb(255, 5, 95, 83)),
            new SolidBrush(Color.FromArgb(255, 146, 8, 21)),
            new SolidBrush(Color.FromArgb(255, 103, 142, 8)),
        };

        int barColourBrushCounter = 0;
        private Brush getNextColour()
        {
            // If no colors are present (because the consumer of this class overrode the bar colours list with an empty one),
            // Default to black
            if (BarColours.Count == 0) { return new SolidBrush(Color.FromArgb(255, 0, 0, 0)); }
            Brush returnMe = BarColours[barColourBrushCounter];
            barColourBrushCounter++;
            if (barColourBrushCounter >= BarColours.Count)
            {
                barColourBrushCounter = 0;
            }
            return returnMe;
        }


        private Dictionary<int, Brush> _barIDColorMappings = new Dictionary<int, Brush>();

        private Brush getBarColour(int id)
        {
            if (!_barIDColorMappings.ContainsKey(id))
            {
                _barIDColorMappings.Add(id, getNextColour());
            }

            return _barIDColorMappings[id];
        }
        
        public byte[] DrawGraph()
        {
            if (this.ChartData.Count == 0)
            {
                throw new NoGraphDataException("No chart data, can't create empty chart.");
            }

            Font font_title = new Font("Arial", 16, FontStyle.Bold);
            Font font_subtitle = new Font("Arial", 12, FontStyle.Bold);
            Font font_subsubtitle = new Font("Arial", 10, FontStyle.Bold);
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
            int titlePadding = 2; // Padding between titles

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
            foreach (BarChartDataSeries label in ChartData)
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

            // Calculate the space we have for bars
            barMaxWidth = Width - largestLabelWidth - labelPadding - barRightEdgePadding;

            // Calculate how much space we need for the title section
            float titleY = 0;
            float subTitleY = 0;
            float subSubTitleY = 0;

            if (!string.IsNullOrEmpty(this.Title))
            {
                SizeF titleSize = graphics.MeasureString(Title, font_title);
                actualTitleAreaSpace += titleSize.Height + titlePadding;
            }

            if (!string.IsNullOrEmpty(this.SubTitle))
            {
                SizeF subTitleSize = graphics.MeasureString(SubTitle, font_subtitle);
                actualTitleAreaSpace += subTitleSize.Height + titlePadding;
                subTitleY = titleY + subTitleSize.Height + titlePadding;
            }

            if (!string.IsNullOrEmpty(this.SubSubTitle))
            {
                SizeF subSubTitleSize = graphics.MeasureString(SubSubTitle, font_subsubtitle);
                actualTitleAreaSpace += subSubTitleSize.Height + titlePadding;
                subSubTitleY = subTitleY + subSubTitleSize.Height + titlePadding;
            }

            actualTitleAreaSpace += titleAfterPadding;


            // Calculate how much space we need for the legend
            float totalLegendHeight = 0;
            if (maxBarsPerSeries > 1)
            {
                totalLegendHeight = legendAfterPadding + (Legend.Count * (legendBoxSize + legendItemPadding));
            }

            // Calculate how much space we'll need for all the bars            
            float totalSeriesHeight = (detectedNumberOfBars * (barHeight + barPadding)) + (seriesPadding * ChartData.Count);

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
            if (maxBarsPerSeries > 1)
            {
                // Calculate maximum legend text width (so we can center everything
                float maxLegendItemWidth = 0;
                foreach (KeyValuePair<int, string> legendItem in Legend)
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
                foreach (KeyValuePair<int, string> legendItem in Legend)
                {
                    Brush thisItemColour = getBarColour(legendItem.Key);

                    // Box
                    graphics.FillRectangle(thisItemColour, legendBoxX, legendYPosition, legendBoxSize, legendBoxSize);

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
            foreach (BarChartDataSeries series in ChartData)
            {
                float thisSeriesY = currentYPosition;

                // Center the label vertically
                float thisLabelY = thisSeriesY + (((barHeight + barPadding) * series.DataPoints.Count) / 2) - barPadding;

                // Draw the label                
                graphics.DrawString(series.Label, font_Label, brush_Label, graphStartX, thisLabelY, new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far });

                // Draw each bar associated with the series
                foreach (BarChartPercentBar bar in series.DataPoints)
                {
                    float thisBarY = currentYPosition;
                    float thisBarMiddleY = thisBarY + (barHeight / 2);

                    // Get the colour for this bar
                    Brush thisBarBrush = getBarColour(bar.ID);

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

            // Return the bitmap as a PNG
            MemoryStream returnedBytes = new MemoryStream();
            bitmap.Save(returnedBytes, ImageFormat.Png);
            return returnedBytes.ToArray();
        }
    }
}

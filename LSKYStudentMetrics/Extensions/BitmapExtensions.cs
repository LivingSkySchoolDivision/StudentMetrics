using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSDMetricsLibrary.Extensions
{
    public static class  BitmapExtensions
    {
        public static Bitmap Crop(this Bitmap b, int Width, int Height)
        {
            Bitmap nb = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(b, 0, 0);
            return nb;
        }
    }
}

using System;
using System.Drawing;

namespace BinPackr
{
    public class Bin
    {
        private const int COLOR_ELEMENT_MIN = 0x20;
        private const int COLOR_ELEMENT_MAX = 0xD0;

        private static readonly Random RandomGenerator = new Random();
        public Rectangle Rectangle { get; set; }
        public Brush Brush { get; set; }

        public System.Drawing.Rectangle DrawingRectangle
        {
            get
            {
                return new System.Drawing.Rectangle(this.Rectangle.X, this.Rectangle.Y, this.Rectangle.Width, this.Rectangle.Height);
            }
        }

        public Bin()
        {
            this.Rectangle = new Rectangle(0, 0, 0, 0);
            this.Brush = RandomBrush();
        }

        public Bin(int x, int y, int width, int height)
        {
            this.Rectangle = new Rectangle(x, y, width, height);
            this.Brush = RandomBrush();
        }

        private static Brush RandomBrush()
        {
            var red = RandomGenerator.Next(COLOR_ELEMENT_MIN, COLOR_ELEMENT_MAX);
            var green = RandomGenerator.Next(COLOR_ELEMENT_MIN, COLOR_ELEMENT_MAX);
            var blue = RandomGenerator.Next(COLOR_ELEMENT_MIN, COLOR_ELEMENT_MAX);

            return new SolidBrush(Color.FromArgb(red, green, blue));
        }
    }
}

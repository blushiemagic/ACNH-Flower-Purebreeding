using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AnimalCrossingFlowers
{
    public static class DrawUtils
    {
        public static Image Cross { get; private set; }
        public static new Image Equals { get; private set; }

        public const int SymbolSize = Flower.IconSize / 2;
        public const int VerticalOffset = (Flower.IconSize - SymbolSize) / 2;
        private const int CrossWidth = SymbolSize / 10;
        private const int EqualsWidth = SymbolSize / 5;

        public static void Init()
        {
            Cross = new Bitmap(SymbolSize, SymbolSize);
            Graphics graphics = Graphics.FromImage(Cross);
            Pen pen = new Pen(Color.Black, CrossWidth);
            int offset = (int)(CrossWidth / 2 / Math.Sqrt(2));
            graphics.DrawLine(pen, offset, offset, SymbolSize - offset, SymbolSize - offset);
            graphics.DrawLine(pen, offset, SymbolSize - offset, SymbolSize - offset, offset);
            pen.Dispose();
            graphics.Dispose();

            Equals = new Bitmap(SymbolSize, SymbolSize);
            graphics = Graphics.FromImage(Equals);
            pen = new Pen(Color.Black, EqualsWidth);
            offset = EqualsWidth / 2;
            graphics.DrawLine(pen, 0, 2 * offset, SymbolSize, 2 * offset);
            graphics.DrawLine(pen, 0, SymbolSize - 2 * offset, SymbolSize, SymbolSize - 2 * offset);
            pen.Dispose();
            graphics.Dispose();
        }

        public static void DrawStringWithBorder(Graphics graphics, String text, Font font,
            Brush brush, Brush borderBrush, float x, float y, int borderWidth)
        {
            for (int i = -borderWidth; i <= borderWidth; i++)
            {
                for (int j = -borderWidth; j <= borderWidth; j++)
                {
                    graphics.DrawString(text, font, borderBrush, x + i, y + j);
                }
            }
            graphics.DrawString(text, font, brush, x, y);
        }

        public static GraphicsPath GetRoundedRectangle(Rectangle rectangle, int radius)
        {
            int diameter = 2 * radius;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180f, 90f);
            path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270f, 90f);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static Image CreateListPanel(IEnumerable<Image> items, Color background, Color border,
            int borderWidth, int minWidth = 0, bool[] highlights = null)
        {
            int width = 0;
            int height = 0;
            foreach (Image item in items)
            {
                if (item.Width > width)
                {
                    width = item.Width;
                }
                height += item.Height;
            }
            width += 2 * borderWidth + 2 * BreedingPair.VerticalSpace;
            height += 2 * borderWidth + BreedingPair.VerticalSpace;
            if (width < minWidth)
            {
                width = minWidth;
            }
            Image panel = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(panel);
            GraphicsPath path = GetRoundedRectangle(new Rectangle(0, 0, width, height), borderWidth);
            Brush brush = new SolidBrush(border);
            graphics.FillPath(brush, path);
            brush.Dispose();
            path.Dispose();
            path = GetRoundedRectangle(new Rectangle(borderWidth, borderWidth,
                width - 2 * borderWidth, height - 2 * borderWidth), borderWidth);
            brush = new SolidBrush(background);
            graphics.FillPath(brush, path);
            brush.Dispose();
            path.Dispose();
            int y = borderWidth + BreedingPair.VerticalSpace;
            int index = 0;
            bool isWhite = background.R > 227 && background.G > 227 && background.B > 227;
            foreach (Image item in items)
            {
                if (highlights != null && highlights[index])
                {
                    path = GetRoundedRectangle(new Rectangle(
                        borderWidth + BreedingPair.VerticalSpace / 2, y - BreedingPair.VerticalSpace / 2,
                        panel.Width - 2 * borderWidth - BreedingPair.VerticalSpace, item.Height
                    ), borderWidth / 2);
                    graphics.FillPath(isWhite ? BreedingPair.DarklightBrush : BreedingPair.HighlightBrush, path);
                    path.Dispose();
                }
                graphics.DrawImage(item, borderWidth + BreedingPair.VerticalSpace, y);
                y += item.Height;
                index++;
            }
            graphics.Dispose();
            return panel;
        }
    }
}

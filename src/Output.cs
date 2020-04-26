using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;

namespace AnimalCrossingFlowers
{
    public static class Output
    {
        private static readonly Color Background = Color.FromArgb(191, 255, 191);
        private static readonly Color TextColor = Color.FromArgb(63, 127, 63);
        private static readonly Font TitleFont = new Font(new FontFamily("Arial Rounded MT Bold"), 128);
        private static readonly Font LegendFont = new Font(new FontFamily("Arial Rounded MT Bold"), 64);
        private const int PanelMargin = 32;

        public static void Write(string path)
        {
            string title = Data.GetName() + " Purebreeding";
            string star = "Purebreeding";
            string seed = "Purchased Seeds";
            string ticket = "Hybrid Mystery Island";
            string genes = "Different Backgrounds = Different Genes";
            List<BreedingPairSection> sections = new List<BreedingPairSection>(Data.GetBreedingPairSections());
            List<Image> sectionPanels = new List<Image>(sections.Select((section) => section.CreateImage()));
            List<Image>[] columns = SplitToColumns(sectionPanels);
            int[] columnWidths = new int[columns.Length];
            int width = -PanelMargin;
            int height = 0;
            for (int k = 0; k < columns.Length; k++)
            {
                List<Image> column = columns[k];
                int columnWidth = 0;
                int columnHeight = -PanelMargin;
                foreach (Image panel in column)
                {
                    if (panel.Width > columnWidth)
                    {
                        columnWidth = panel.Width;
                    }
                    columnHeight += panel.Height + PanelMargin;
                }
                columnWidths[k] = columnWidth;
                width += columnWidth + PanelMargin;
                if (columnHeight > height)
                {
                    height = columnHeight;
                }
            }

            Image content = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(content);
            int x = 0;
            int y = 0;
            int index = 0;
            for (int k = 0; k < columns.Length; k++)
            {
                List<Image> column = columns[k];
                int columnWidth = columnWidths[k];
                y = 0;
                foreach (Image panel in column)
                {
                    if (panel.Width < columnWidth)
                    {
                        graphics.DrawImage(sections[index].CreateImage(columnWidth), x, y);
                    }
                    else
                    {
                        graphics.DrawImage(panel, x, y);
                    }
                    y += panel.Height + PanelMargin;
                    index++;
                }
                x += columnWidth + PanelMargin;
            }

            SizeF titleSize = graphics.MeasureString(title, TitleFont);
            SizeF starSize = graphics.MeasureString(star, LegendFont);
            SizeF seedSize = graphics.MeasureString(seed, LegendFont);
            SizeF ticketSize = graphics.MeasureString(ticket, LegendFont);
            SizeF genesSize = graphics.MeasureString(genes, LegendFont);

            graphics.Dispose();
            width = 4 * PanelMargin + Math.Max(content.Width, (int)titleSize.Width);
            height = 9 * PanelMargin + (int)titleSize.Height + (int)starSize.Height
                + (int)seedSize.Height + (int)ticketSize.Height + (int)genesSize.Height + content.Height;
            Image image = new Bitmap(width, height);
            graphics = Graphics.FromImage(image);
            graphics.Clear(Background);
            Brush brush = new SolidBrush(TextColor);
            Brush borderBrush = new SolidBrush(Color.White);
            x = (image.Width - (int)titleSize.Width) / 2;
            y = 2 * PanelMargin;
            DrawUtils.DrawStringWithBorder(graphics, title, TitleFont, brush, borderBrush, x, y, 8);
            x = image.Width / 4;
            y += (int)titleSize.Height + PanelMargin;
            Sprites.DrawSprite(graphics, x - Sprites.SpriteSize - PanelMargin / 2,
                y + ((int)starSize.Height - Sprites.SpriteSize) / 2, Flower.Star);
            DrawUtils.DrawStringWithBorder(graphics, star, LegendFont, brush, borderBrush, x + PanelMargin / 2, y, 4);
            y += (int)starSize.Height + PanelMargin;
            Sprites.DrawSprite(graphics, x - Sprites.SpriteSize - PanelMargin / 2,
                y + ((int)seedSize.Height - Sprites.SpriteSize) / 2, Flower.GetSource("seedred"));
            DrawUtils.DrawStringWithBorder(graphics, seed, LegendFont, brush, borderBrush, x + PanelMargin / 2, y, 4);
            y += (int)seedSize.Height + PanelMargin;
            Sprites.DrawSprite(graphics, x - Sprites.SpriteSize - PanelMargin / 2,
                y + ((int)ticketSize.Height - Sprites.SpriteSize) / 2, Flower.GetSource("island"));
            DrawUtils.DrawStringWithBorder(graphics, ticket, LegendFont, brush, borderBrush, x + PanelMargin / 2, y, 4);
            y += (int)ticketSize.Height + PanelMargin;
            x = (image.Width - (int)genesSize.Width) / 2;
            DrawUtils.DrawStringWithBorder(graphics, genes, LegendFont, brush, borderBrush, x, y, 4);
            y += (int)genesSize.Height + PanelMargin;
            brush.Dispose();
            borderBrush.Dispose();
            graphics.DrawImage(content, (image.Width - content.Width) / 2, y);
            graphics.Dispose();

            image.Save(path + ".png", ImageFormat.Png);
        }

        private static List<Image>[] SplitToColumns(List<Image> panels)
        {
            List<Image> sectionPanels = new List<Image>(Data.GetBreedingPairSections().Select((section) => section.CreateImage()));
            int width = 0;
            int height = 0;
            foreach (Image sectionPanel in sectionPanels)
            {
                if (sectionPanel.Width > width)
                {
                    width = sectionPanel.Width;
                }
                height += sectionPanel.Height;
            }
            int numColumns = sectionPanels.Count;
            while (numColumns > 1 && width * numColumns > height / numColumns)
            {
                numColumns--;
            }
            List<Image>[] columns = new List<Image>[numColumns];
            for (int k = 0; k < columns.Length; k++)
            {
                columns[k] = new List<Image>();
            }
            int difference = panels.Count - columns.Length;
            for (int k = 0; k < difference; k++)
            {
                columns[0].Add(panels[k]);
            }
            for (int k = 0; k < columns.Length; k++)
            {
                columns[k].Add(panels[difference + k]);
            }
            bool tryMore = true;
            while (tryMore)
            {
                int index = -1;
                double score = ScoreColumns(columns);
                for (int k = 0; k < columns.Length - 1; k++)
                {
                    List<Image> column = columns[k];
                    if (column.Count > 1)
                    {
                        Image item = column[column.Count - 1];
                        column.RemoveAt(column.Count - 1);
                        columns[k + 1].Insert(0, item);
                        double tryScore = ScoreColumns(columns);
                        if (tryScore < score)
                        {
                            index = k;
                            score = tryScore;
                        }
                        columns[k + 1].RemoveAt(0);
                        column.Add(item);
                    }
                }
                if (index >= 0)
                {
                    Image item = columns[index][columns[index].Count - 1];
                    columns[index].RemoveAt(columns[index].Count - 1);
                    columns[index + 1].Insert(0, item);
                }
                else
                {
                    tryMore = false;
                }
            }
            return columns;
        }

        private static double ScoreColumns(List<Image>[] columns)
        {
            int[] heights = columns.Select((panels) =>
            {
                int total = -PanelMargin;
                foreach (Image panel in panels)
                {
                    total += panel.Height + PanelMargin;
                }
                return total;
            }).ToArray();
            double mean = 0;
            foreach (int item in heights)
            {
                mean += item;
            }
            mean /= heights.Length;
            double variance = 0;
            foreach (int item in heights)
            {
                variance += Math.Pow(item - mean, 2);
            }
            return variance;
        }
    }
}

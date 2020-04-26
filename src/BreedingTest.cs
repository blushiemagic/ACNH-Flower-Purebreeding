using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;

namespace AnimalCrossingFlowers
{
    public class BreedingTest
    {
        private static readonly Font TestFont = new Font(new FontFamily("Arial Rounded MT Bold"), Sprites.SpriteSize / 2);
        private static readonly Brush TestBrush = new SolidBrush(Color.Black);
        private static readonly Brush TestBorderBrush = new SolidBrush(Color.White);
        private static readonly Color PanelBackground = Color.White;
        private static readonly Color PanelBorder = Color.FromArgb(63, 63, 63);
        private const int PanelBorderWidth = 16;

        private List<TestPair> testPairs;

        public BreedingTest()
        {
            this.testPairs = new List<TestPair>();
        }

        public void AddTestPair(TestPair pair)
        {
            testPairs.Add(pair);
        }

        public Image CreateImage(Brush testBrush = null)
        {
            //TODO
            IEnumerable<Image> pairImages = new List<Image>(testPairs.Select((pair) => pair.CreateImage()));
            Image panel = DrawUtils.CreateListPanel(pairImages, PanelBackground, PanelBorder, PanelBorderWidth);
            string text = "Test:";
            Graphics graphics = Graphics.FromImage(panel);
            SizeF textSize = graphics.MeasureString(text, TestFont);
            graphics.Dispose();
            int width = (int)textSize.Width + BreedingPair.HorizontalSpace + panel.Width / 2;
            int height = Math.Max(BreedingPair.VerticalSpace + (int)textSize.Height, panel.Height / 2);
            Image image = new Bitmap(width, height);
            graphics = Graphics.FromImage(image);
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            if (testBrush == null)
            {
                testBrush = TestBrush;
            }
            DrawUtils.DrawStringWithBorder(graphics, text, TestFont, testBrush, TestBorderBrush, 0, BreedingPair.VerticalSpace, 4);
            graphics.DrawImage(panel, (int)textSize.Width + BreedingPair.HorizontalSpace, 0, panel.Width / 2, panel.Height / 2);
            graphics.Dispose();
            return image;
        }
    }

    public class TestPair
    {
        private Flower flower1;
        private Flower flower2;
        private List<TestPairResult> results;

        public TestPair(Flower flower1, Flower flower2)
        {
            this.flower1 = flower1;
            this.flower2 = flower2;
            this.results = new List<TestPairResult>();
        }

        public void AddTestResult(FlowerColor color, int chance)
        {
            results.Add(new TestPairResult(color, chance));
        }

        public Image CreateImage()
        {
            int width = 2 * Flower.IconSize + 2 * DrawUtils.SymbolSize + 3 * BreedingPair.HorizontalSpace;
            width += results.Count * (BreedingPair.HorizontalSpace + Sprites.SpriteSize);
            int height = Math.Max(Flower.IconSize, (Flower.IconSize + Sprites.SpriteSize) / 2 + BreedingPair.ChanceSpace);
            height += BreedingPair.VerticalSpace;
            Image image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            int x = 0;
            graphics.DrawImage(flower1.Icon, x, 0);
            x += Flower.IconSize + BreedingPair.HorizontalSpace;
            graphics.DrawImage(DrawUtils.Cross, x, DrawUtils.VerticalOffset);
            x += DrawUtils.SymbolSize + BreedingPair.HorizontalSpace;
            graphics.DrawImage(flower2.Icon, x, 0);
            x += Flower.IconSize + BreedingPair.HorizontalSpace;
            graphics.DrawImage(DrawUtils.Equals, x, DrawUtils.VerticalOffset);
            x += DrawUtils.SymbolSize + BreedingPair.HorizontalSpace;
            foreach (TestPairResult result in results)
            {
                result.Color.DrawSprite(graphics, x, (Flower.IconSize - Sprites.SpriteSize) / 2);
                string chanceText = result.Chance + "%";
                SizeF size = graphics.MeasureString(chanceText, BreedingPair.ChanceFont);
                float chanceOffset = (Sprites.SpriteSize - size.Width) / 2f;
                graphics.DrawString(chanceText, BreedingPair.ChanceFont, BreedingPair.ChanceBrush,
                    x + chanceOffset, (Flower.IconSize + Sprites.SpriteSize) / 2 + BreedingPair.ChanceTopSpace);
                x += Sprites.SpriteSize + BreedingPair.HorizontalSpace;
            }
            graphics.Dispose();
            return image;
        }
    }

    public class TestPairResult
    {
        public readonly FlowerColor Color;
        public readonly int Chance;

        public TestPairResult(FlowerColor color, int chance)
        {
            this.Color = color;
            this.Chance = chance;
        }
    }
}

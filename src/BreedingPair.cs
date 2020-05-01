using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace AnimalCrossingFlowers
{
    public class BreedingPair
    {
        public const int VerticalSpace = 32;
        public const int HorizontalSpace = 16;
        public const int ChanceSpace = VerticalSpace;
        public const int ChanceTopSpace = 8;
        public static readonly Font ChanceFont = new Font(new FontFamily("Arial Rounded MT Bold"), ChanceSpace - ChanceTopSpace);
        public static readonly Brush ChanceBrush = new SolidBrush(Color.Black);
        private static readonly Font NotPossibleFont = new Font(new FontFamily("Arial Rounded MT Bold"), Sprites.SpriteSize / 4);
        private static readonly Brush NotPossibleBrush = new SolidBrush(Color.Black);
        private static readonly Brush NotPossibleBorderBrush = new SolidBrush(Color.White);
        public static readonly Brush HighlightBrush = new SolidBrush(Color.FromArgb(127, 255, 255, 255));
        public static readonly Brush DarklightBrush = new SolidBrush(Color.FromArgb(127, 200, 200, 200));

        private Flower flower1;
        private Flower flower2;
        private List<BreedingResult> results;
        private List<BreedingTest> tests;

        public bool Impossible => flower2 == null;

        public BreedingPair(Flower flower1, Flower flower2)
        {
            this.flower1 = flower1;
            this.flower2 = flower2;
            this.results = new List<BreedingResult>();
            this.tests = new List<BreedingTest>();
        }

        public BreedingPair(Flower impossible)
        {
            this.flower1 = impossible;
        }

        public void AddBreedingResult(Flower flower, int chance)
        {
            results.Add(new BreedingResult(flower, chance));
        }

        public void AddBreedingTest(BreedingTest test)
        {
            tests.Add(test);
        }

        public Image CreateImage()
        {
            Graphics graphics;
            int width, height;
            Image image;
            if (Impossible)
            {
                Image icon = flower1.Icon;
                string text1 = "Impossible to test for";
                string text2 = " with 100% certainty";
                graphics = Graphics.FromImage(icon);
                SizeF size1 = graphics.MeasureString(text1, NotPossibleFont);
                SizeF size2 = graphics.MeasureString(text2, NotPossibleFont);
                graphics.Dispose();
                width = (int)Math.Max(size1.Width, size2.Width) + 4;
                height = (int)size1.Height + (int)NotPossibleFont.Size / 2 + (int)size2.Height + 4;
                Image textImage = new Bitmap(width, height);
                graphics = Graphics.FromImage(textImage);
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                DrawUtils.DrawStringWithBorder(graphics, text1, NotPossibleFont, NotPossibleBrush, NotPossibleBorderBrush,
                    2, 2, 2);
                DrawUtils.DrawStringWithBorder(graphics, text2, NotPossibleFont, NotPossibleBrush, NotPossibleBorderBrush,
                    2, 2 + (int)size1.Height + (int)NotPossibleFont.Size / 2, 2);
                graphics.Dispose();
                width = Flower.IconSize * 5 / 4 + textImage.Width;
                height = Flower.IconSize + VerticalSpace;
                image = new Bitmap(width, height);
                graphics = Graphics.FromImage(image);
                graphics.DrawImage(icon, 0, 0);
                graphics.DrawImage(textImage, Flower.IconSize * 5 / 4, (Flower.IconSize - textImage.Height) / 2);
                graphics.Dispose();
            }
            else
            {
                IEnumerable<Image> testImages = new List<Image>(tests.Select((test) => test.CreateImage()));
                width = 2 * Flower.IconSize + 2 * DrawUtils.SymbolSize + 3 * HorizontalSpace;
                width += results.Count * (HorizontalSpace + Flower.IconSize);
                height = Flower.IconSize + ChanceSpace + VerticalSpace;
                foreach (Image testImage in testImages)
                {
                    height += testImage.Height + VerticalSpace;
                }

                image = new Bitmap(width, height);
                graphics = Graphics.FromImage(image);
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                int x = 0;
                graphics.DrawImage(flower1.Icon, x, 0);
                x += Flower.IconSize + HorizontalSpace;
                graphics.DrawImage(DrawUtils.Cross, x, DrawUtils.VerticalOffset);
                x += DrawUtils.SymbolSize + HorizontalSpace;
                graphics.DrawImage(flower2.Icon, x, 0);
                x += Flower.IconSize + HorizontalSpace;
                graphics.DrawImage(DrawUtils.Equals, x, DrawUtils.VerticalOffset);
                x += DrawUtils.SymbolSize + HorizontalSpace;
                foreach (BreedingResult result in results)
                {
                    graphics.DrawImage(result.Flower.Icon, x, 0);
                    string chanceText = result.Chance + "%";
                    SizeF size = graphics.MeasureString(chanceText, ChanceFont);
                    float chanceOffset = (Flower.IconSize - size.Width) / 2f;
                    graphics.DrawString(chanceText, ChanceFont, ChanceBrush, x + chanceOffset, Flower.IconSize + ChanceTopSpace);
                    x += Flower.IconSize + HorizontalSpace;
                }

                int y = Flower.IconSize + ChanceSpace + VerticalSpace;
                foreach (Image testImage in testImages)
                {
                    graphics.DrawImage(testImage, 0, y);
                    y += testImage.Height + VerticalSpace;
                }

                graphics.Dispose();
            }
            return image;
        }

        public bool ContainsTarget(IEnumerable<string> targets)
        {
            foreach (BreedingResult result in results)
            {
                foreach (string target in targets)
                {
                    if (result.Flower.ID == target)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class BreedingResult
    {
        public readonly Flower Flower;
        public readonly int Chance;

        public BreedingResult(Flower flower, int chance)
        {
            this.Flower = flower;
            this.Chance = chance;
        }
    }

    public class BreedingPairSection
    {
        private const int BorderWidth = 16;
        private static readonly Dictionary<string, Color> backgrounds = new Dictionary<string, Color>();
        private static readonly Dictionary<string, Color> borders = new Dictionary<string, Color>();
        private static bool init = false;

        public readonly IEnumerable<string> Targets;
        public readonly string BackgroundColor;
        public readonly string BorderColor;
        public readonly IEnumerable<BreedingPair> BreedingPairs;

        public BreedingPairSection(IEnumerable<BreedingPair> pairs, IEnumerable<string> targets, string backgroundColor, string borderColor = null)
        {
            this.Targets = targets;
            this.BackgroundColor = backgroundColor;
            this.BorderColor = borderColor;
            this.BreedingPairs = pairs;
            if (!init)
            {
                Init();
            }
        }

        public Image CreateImage(int minWidth = 0)
        {
            IEnumerable<Image> pairImages = new List<Image>(BreedingPairs.Select((pair) => pair.CreateImage()));
            bool[] highlights = BreedingPairs.Select((pair) => !pair.Impossible && pair.ContainsTarget(Targets)).ToArray();
            string background = BackgroundColor;
            string border = BorderColor ?? BackgroundColor;
            return DrawUtils.CreateListPanel(pairImages, backgrounds[background], borders[border], BorderWidth, minWidth, highlights);
        }

        private static void Init()
        {
            backgrounds["red"] = Color.FromArgb(255, 191, 191);
            borders["red"] = Color.FromArgb(127, 63, 63);
            backgrounds["orange"] = Color.FromArgb(255, 223, 191);
            borders["orange"] = Color.FromArgb(127, 95, 63);
            backgrounds["yellow"] = Color.FromArgb(255, 255, 191);
            borders["yellow"] = Color.FromArgb(127, 127, 63);
            backgrounds["green"] = Color.FromArgb(191, 255, 191);
            borders["green"] = Color.FromArgb(63, 127, 63);
            backgrounds["pink"] = Color.FromArgb(255, 191, 223);
            borders["pink"] = Color.FromArgb(127, 63, 95);
            backgrounds["white"] = Color.FromArgb(255, 255, 255);
            borders["white"] = Color.FromArgb(127, 127, 127);
            backgrounds["blue"] = Color.FromArgb(191, 191, 255);
            borders["blue"] = Color.FromArgb(63, 63, 127);
            backgrounds["purple"] = Color.FromArgb(255, 191, 255);
            borders["purple"] = Color.FromArgb(127, 63, 127);
            backgrounds["black"] = Color.FromArgb(191, 191, 191);
            borders["black"] = Color.FromArgb(63, 63, 63);
            init = true;
        }
    }
}

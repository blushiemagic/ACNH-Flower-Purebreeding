using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AnimalCrossingFlowers
{
    public class Flower
    {
        public const int IconSize = Sprites.SpriteSize + 2 * BorderWidth + 16;
        private const int BorderWidth = 8;

        public static readonly Point Star = new Point(3, 8);
        private static readonly Brush borderBrush = new SolidBrush(System.Drawing.Color.FromArgb(63, 63, 63));
        private static readonly Dictionary<string, Color> backgrounds = new Dictionary<string, Color>();
        private static readonly Dictionary<string, Point> sources = new Dictionary<string, Point>();
        private static bool init = false;

        public readonly string ID;
        public readonly string Color;
        public readonly string Background;
        public readonly bool Purebred;
        public readonly string Source;
        public readonly Image Icon;

        public Flower(string id, string color, string background, bool pure, string source)
        {
            if (id == "-" || id == "!")
            {
                throw new InputException("Flower ID cannot be \"-\" or \"!\"");
            }
            ID = id;
            Color = color;
            Background = background;
            Purebred = pure;
            Source = source;

            if (!init)
            {
                Init();
            }
            string background1 = Background;
            string background2 = null;
            if (background1.IndexOf('-') >= 0)
            {
                int index = background1.IndexOf('-');
                background1 = background.Substring(0, index);
                background2 = background.Substring(index + 1);
            }
            if (!backgrounds.ContainsKey(background1))
            {
                throw new InputException("Unknown background color: " + background1);
            }
            if (background2 != null && !backgrounds.ContainsKey(background2))
            {
                throw new InputException("Unknown background color: " + background2);
            }
            if (source != null && !sources.ContainsKey(Source))
            {
                throw new InputException("Unknown flower source: " + Source);
            }

            Icon = new Bitmap(IconSize, IconSize);
            Graphics graphics = Graphics.FromImage(Icon);
            graphics.FillEllipse(borderBrush, 0, 0, IconSize, IconSize);
            Brush fillBrush;
            if (background2 == null)
            {
                fillBrush = new SolidBrush(backgrounds[background1]);
            }
            else
            {
                fillBrush = new LinearGradientBrush(new Point(BorderWidth, IconSize / 2),
                    new Point(IconSize - BorderWidth, IconSize / 2),
                    backgrounds[background1], backgrounds[background2]);
            }
            graphics.FillEllipse(fillBrush, BorderWidth, BorderWidth, IconSize - 2 * BorderWidth, IconSize - 2 * BorderWidth);
            fillBrush.Dispose();
            Data.GetFlowerColor(Color).DrawSprite(graphics, (IconSize - Sprites.SpriteSize) / 2, (IconSize - Sprites.SpriteSize) / 2);
            if (source != null)
            {
                Sprites.DrawSprite(graphics, 0, IconSize - Sprites.SpriteSize / 2, 0.5f, sources[Source]);
            }
            if (pure)
            {
                Sprites.DrawSprite(graphics, IconSize - Sprites.SpriteSize / 2, IconSize - Sprites.SpriteSize / 2, 0.5f, Star);
            }
            graphics.Dispose();
        }

        private static void Init()
        {
            backgrounds["white"] = System.Drawing.Color.FromArgb(255, 255, 255);
            backgrounds["red"] = System.Drawing.Color.FromArgb(255, 127, 127);
            backgrounds["yellow"] = System.Drawing.Color.FromArgb(255, 255, 127);
            backgrounds["blue"] = System.Drawing.Color.FromArgb(127, 127, 255);
            backgrounds["orange"] = System.Drawing.Color.FromArgb(255, 191, 127);
            backgrounds["purple"] = System.Drawing.Color.FromArgb(255, 127, 255);
            backgrounds["green"] = System.Drawing.Color.FromArgb(127, 255, 127);
            backgrounds["grey"] = System.Drawing.Color.FromArgb(191, 191, 191);
            backgrounds["deepyellow"] = System.Drawing.Color.FromArgb(223, 223, 0);
            backgrounds["deepblue"] = System.Drawing.Color.FromArgb(0, 0, 255);
            backgrounds["deepred"] = System.Drawing.Color.FromArgb(255, 0, 0);
            backgrounds["deepgreen"] = System.Drawing.Color.FromArgb(0, 223, 0);

            sources["seedred"] = new Point(0, 10);
            sources["seedorange"] = new Point(2, 10);
            sources["seedyellow"] = new Point(1, 10);
            sources["seedwhite"] = new Point(20, 9);
            sources["island"] = new Point(5, 4);

            init = true;
        }

        public static Point GetSource(string id)
        {
            return sources[id];
        }
    }
}

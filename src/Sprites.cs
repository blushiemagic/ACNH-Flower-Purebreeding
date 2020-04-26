using System;
using System.Drawing;
using System.Reflection;

namespace AnimalCrossingFlowers
{
    public static class Sprites
    {
        public const int SpriteSize = 128;
        private const int SpriteGap = 5;

        private static Image sheet;

        private static void Init()
        {
            sheet = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("AnimalCrossingFlowers.sprites.png"));
        }

        private static int GetPos(int index)
        {
            return SpriteGap + index * (SpriteSize + SpriteGap);
        }

        public static void DrawSprite(Graphics graphics, int drawX, int drawY, int x, int y)
        {
            if (sheet == null)
            {
                Init();
            }
            graphics.DrawImage(sheet, drawX, drawY, new Rectangle(GetPos(x), GetPos(y), SpriteSize - 1, SpriteSize - 1), GraphicsUnit.Pixel);
        }

        public static void DrawSprite(Graphics graphics, int drawX, int drawY, Point index)
        {
            DrawSprite(graphics, drawX, drawY, index.X, index.Y);
        }

        public static void DrawSprite(Graphics graphics, int drawX, int drawY, float scale, Point index)
        {
            if (sheet == null)
            {
                Init();
            }
            int drawSize = (int)(scale * SpriteSize);
            graphics.DrawImage(sheet, new Rectangle(drawX, drawY, drawSize, drawSize),
                new Rectangle(GetPos(index.X), GetPos(index.Y), SpriteSize - 1, SpriteSize - 1), GraphicsUnit.Pixel);
        }

        public static Image GetSprite(int x, int y)
        {
            Image sprite = new Bitmap(SpriteSize, SpriteSize);
            Graphics graphics = Graphics.FromImage(sprite);
            DrawSprite(graphics, 0, 0, x, y);
            graphics.Dispose();
            return sprite;
        }

        public static Image GetSprite(Point index)
        {
            return GetSprite(index.X, index.Y);
        }
    }
}

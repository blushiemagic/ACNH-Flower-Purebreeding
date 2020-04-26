using System;
using System.Collections.Generic;
using System.Drawing;

namespace AnimalCrossingFlowers
{
    public class FlowerColor
    {
        public readonly string ID;
        public readonly int X;
        public readonly int Y;

        public Point Index => new Point(X, Y);

        public FlowerColor(string id, int x, int y)
        {
            ID = id;
            X = x;
            Y = y;
        }

        public void DrawSprite(Graphics graphics, int drawX, int drawY)
        {
            Sprites.DrawSprite(graphics, drawX, drawY, X, Y);
        }

        public Image GetSprite()
        {
            return Sprites.GetSprite(X, Y);
        }
    }
}

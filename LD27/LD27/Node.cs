#region Using
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
#endregion

namespace Tracewire
{
    public class Node
    {
        public int Minimum = 0;
        public int Color;
        public Point Connection;
        public bool Faded;
        public int Colorize = 0;
        public bool SpecialType = false;
        public bool SpecialTypePassed = false;

        public float Angle;
        public float Velocity;
        public Vector2 Position;

        public bool Exploding = false;

        public Node(int Color, Point Connection, bool Special)
        {
            this.Faded = false;
            this.Color = Color;
            this.Connection = Connection;
            this.SpecialType = Special;

            this.Angle = (float)Game1.GRand.NextDouble()*2;
        }
    }
}

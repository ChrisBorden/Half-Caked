﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Half_Caked
{
    public enum Surface
    {
        Antiportal = 5,
        Absorbs = 4,
        Amplifies = 3,
        Normal = 2,
        Reflects = 1,
        Death = 0
    }

    public class Tile
    {
        #region Fields
        public Rectangle Dimensions;
        public Surface Type;

        public float Friction
        {
            get { return ((int)Type) * .20f; }
        }
        #endregion

        #region Initialization
        public Tile() 
            : this(Rectangle.Empty, Surface.Death)
        {
        }

        public Tile(Rectangle shape, Surface type)
        {
            Dimensions = shape;
            Type = type;
        }
        #endregion
    }
}

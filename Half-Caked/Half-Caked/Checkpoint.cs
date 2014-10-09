﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    public class Checkpoint
    {
        #region Fields
        public Vector2 Location, Quadrant, Bound;
        public string NarrationText;
        #endregion

        #region Initialization
        public Checkpoint()
        {
        }

        public Checkpoint(int x, int y, int xBound, int yBound, int quadrant, string text) 
            : this(new Vector2(x,y), new Vector2(xBound, yBound), quadrant, text)
        {
        }

        public Checkpoint(Vector2 loc, Vector2 boundary, int quadrant, string text)
        {
            Location = loc;
            Bound = boundary;
            NarrationText = text;

            Quadrant = new Vector2(quadrant % 3 == 1 ? -1 : 1, quadrant > 2 ? -1 : 1);
        }
        #endregion

        #region Public Methods
        public bool InBounds(Vector2 loc)
        {
            var result = Quadrant * (Bound - loc);

            return result.X >= 0 && result.Y >= 0;
        }
        #endregion
    }
}

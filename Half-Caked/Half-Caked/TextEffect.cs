using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Half_Caked
{
    public abstract class TextEffect
    {
        public bool Done;

        public abstract void Draw(SpriteBatch theSpriteBatch, SpriteFont font);
        public abstract void Update(GameTime theGameTime);
    }

    public class CheckpointNotification : TextEffect
    {
        #region Constants
        private const float VELOCITY_Y = -50;
        private const float TIME_TO_FADE = 2;
        #endregion

        #region Fields
        private Color mColor = Color.Gold;
        private Vector2 mPosition;
        #endregion

        #region Initialization
        public CheckpointNotification(Vector2 pos)
        {
            mPosition = pos;
        }
        #endregion

        #region Update and Draw
        public override void Update(GameTime theGameTime)
        {
            if (mColor.A == 0)
                Done = true;

            mColor.A = (byte)MathHelper.Clamp(mColor.A - (float)theGameTime.ElapsedGameTime.TotalSeconds * 255 / TIME_TO_FADE, 0, 255);
            mPosition.Y += VELOCITY_Y * (float)theGameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch theSpriteBatch, SpriteFont font)
        {
            theSpriteBatch.DrawString(font, "Checkpointed!", mPosition, mColor, 0,
                                  Vector2.Zero, .5f, SpriteEffects.None, 0);
        }
        #endregion
    }

    public class DeathNotification : TextEffect
    {
        #region Constants
        private const float VELOCITY_Y = -100;
        private const float VELOCITY_X = -50;
        private const float TIME_TO_FADE = 2;
        #endregion

        #region Fields
        private Color mColor = Color.Red;
        private Vector2 mPosition;
        private float mBaseX;
        #endregion

        #region Initialization
        public DeathNotification(Vector2 pos)
        {
            mPosition = pos;
            mBaseX = pos.X;
        }
        #endregion

        #region Update and Draw
        public override void Update(GameTime theGameTime)
        {
            if (mColor.A == 0)
                Done = true;

            mColor.A = (byte)MathHelper.Clamp(mColor.A - (float)theGameTime.ElapsedGameTime.TotalSeconds * 255 / TIME_TO_FADE, 0, 255);
            mPosition.Y += VELOCITY_Y * (float)theGameTime.ElapsedGameTime.TotalSeconds;
            mPosition.X = VELOCITY_X * (float)Math.Sin(theGameTime.TotalGameTime.TotalSeconds * 3 * (MathHelper.Pi)) + mBaseX;
        }

        public override void Draw(SpriteBatch theSpriteBatch, SpriteFont font)
        {
            theSpriteBatch.DrawString(font, "You died!", mPosition, mColor, 0,
                                  Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
        #endregion
    }
}

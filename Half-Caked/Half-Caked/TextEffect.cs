using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Speech.Synthesis; 

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

    public class NarrationEffect : TextEffect
    {
        #region Constants
        private const float TIME_TO_FADE = 4;
        private const float TIME_PER_CHAR = .05f;
        private const float FONT_SCALE = 1f;
        #endregion

        #region Fields
        SpeechSynthesizer mNarrator;
        private bool mFirst = true;
        private SpriteFont mFont;
        private Color mTextColor = Color.White;
        private Rectangle mRectOut;
        private Texture2D mBackground, mNarrationImage;
        private string[] mWords;
        private List<string> mCurText = new List<string>(){""};
        private int mIndex = 0, mWordIndex = 0, mRowIndex = 0;
        private float mCurTime = 0, mTextXStart;
        #endregion
        
        #region Initialization
        public NarrationEffect(string text, ScreenManager manager)
        {
            mNarrator = manager.GetScreens().OfType<GameplayScreen>().First().Narrator;
            mNarrator.Volume = (manager.Game as HalfCakedGame).CurrentProfile.Audio.NarrationVolume * (manager.Game as HalfCakedGame).CurrentProfile.Audio.MasterVolume / 100;
            mNarrator.Rate = 1;

            mWords = text.Split(' ').Select( x => x + " " ).ToArray();
            mFont = manager.Font;

            mBackground = manager.Game.Content.Load<Texture2D>(@"UI\NarrationBackground");
            mNarrationImage = manager.Game.Content.Load<Texture2D>(@"UI\Narrator");

            var size = manager.GraphicsDevice.Viewport.Bounds;
            mRectOut = new Rectangle(size.Width/8, 10, size.Width * 3 / 4, mNarrationImage.Height + 20);

            mTextXStart = mRectOut.X + mNarrationImage.Width + 30;
        }
        #endregion

        #region Update and Draw
        public override void Update(GameTime theGameTime)
        {
            mCurTime += (float)theGameTime.ElapsedGameTime.TotalSeconds;

            if (mWordIndex >= mWords.Length)
            {
                Done = mCurTime > TIME_TO_FADE;
                return;
            }

            while (mCurTime > TIME_PER_CHAR)
            {
                mCurTime -= TIME_PER_CHAR;

                if (mIndex >= mWords[mWordIndex].Length)
                {
                    mWordIndex++;
                    mIndex = 0;
                    if (mWordIndex >= mWords.Length)
                        return;

                    if (mFont.MeasureString(mCurText[mRowIndex] + mWords[mWordIndex].Trim()).X * FONT_SCALE > mRectOut.Right - mTextXStart - 10)
                    {
                        mRowIndex++;
                        mCurText.Add("");
                    }
                }

                mCurText[mRowIndex] += mWords[mWordIndex][mIndex++];             
            }

            float mHeight = 0;
            foreach (string s in mCurText)
                mHeight += mFont.MeasureString(s).Y * FONT_SCALE;

            if (mHeight + 20 > mRectOut.Height)
            {
                mCurText.RemoveAt(0);
                mRowIndex--;
            }
        }

        public override void Draw(SpriteBatch theSpriteBatch, SpriteFont font)
        {
            if (mFirst)
            {
                mFirst = false;
                mNarrator.SpeakAsync(mWords.Aggregate((x, y) => { return x + y; }));
            }

            theSpriteBatch.Draw(mBackground, mRectOut, Color.White);
            theSpriteBatch.Draw(mNarrationImage, new Vector2(mRectOut.X + 20, mRectOut.Y + 10), Color.White);

            Vector2 textPos = new Vector2(mTextXStart, 20);

            foreach (string s in mCurText)
            {
                theSpriteBatch.DrawString(mFont, s, textPos, mTextColor, 0,
                                      Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);
                textPos.Y += mFont.LineSpacing * FONT_SCALE;
            }
        }
        #endregion
    }
}

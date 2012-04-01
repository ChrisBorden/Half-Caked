using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

//CUSTOM CODE

namespace Half_Caked
{
    class Button : UIElement
    {
        #region Fields

        private string mText;
        public string Text
        {
            get { return mText; }
            set
            {
                mText = value;
                Vector2 mTextSize = UIFont.MeasureString(mText);
                mRectangle = new Rectangle(0, 0, 0, 0);
                mRectangle.Width += (int)mTextSize.X;
                mRectangle.Height += (int)Math.Max(mTextSize.Y, UIFont.MeasureString(" ").Y);

                mOrigin = new Vector2(mTextSize.X / 2 * (int)Alignment, mTextSize.Y / 2);
                Refresh = true;
            }
        }

        private Rectangle mSource;
        private Color mTextColor, mDimColor;

        public static Color SELECTED_COLOR = Color.FromNonPremultiplied(79, 129, 184, 255);

        public override UIState State
        {
            get { return mState; }
            set
            {
                mState = value;
                switch (mState)
                {
                    case UIState.Selected:
                        mTextColor = SELECTED_COLOR;
                        mDimColor = Color.White;
                        mSource = new Rectangle(250, 0, 250, 100);
                        break;
                    case UIState.Inactive:
                        mTextColor = Color.DimGray;
                        mDimColor = Color.DimGray;
                        mSource = new Rectangle(0, 0, 250, 100);
                        break;
                    default:
                        mTextColor = Color.White;
                        mDimColor = Color.White;
                        mSource = new Rectangle(0, 0, 250, 100);
                        break;

                }
            }
        }

        private Texture2D mBackground;

        #endregion

        public Button(string txt)
            : this(txt, 10, 5, Alignment.Centered)
        {
        }

        public Button(string txt, int xPad, int yPad, Alignment algn)
        {
            mText = txt;
            mPaddingX = xPad;
            mPaddingY = yPad;
            mRectangle = new Rectangle(0, 0, 0, 0);
            Alignment = algn;
            State = UIState.Active;
        }

        public override bool HandleMouseInput(InputState input)
        {
            var paintedArea = mRectangle;
            paintedArea.Y -= (int)Padding.Y;
            paintedArea.X -= (int)Padding.X;
            paintedArea.Height += 2*(int)Padding.Y;
            paintedArea.Width += 2 * (int)Padding.X;

            if (paintedArea.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y) && State != UIState.Inactive)
            {
                if (!input.IsNewMouseState())
                    return false;
                else if (input.IsNewLeftMouseClick() || input.IsNewRightMouseClick())
                {
                    OnPressedElement(PlayerIndex.One, 0);
                }
                return true;
            }

            return false;
        }

        public override void LoadContent(GameScreen scrn)
        {
            base.LoadContent(scrn);

            mBackground = scrn.ScreenManager.Game.Content.Load<Texture2D>("UI\\Buttons");

            Vector2 mTextSize = UIFont.MeasureString(Text);
            mRectangle.Width  = (int)mTextSize.X;
            mRectangle.Height = (int)Math.Max(mTextSize.Y, UIFont.MeasureString(" ").Y);

            mOrigin = new Vector2(mTextSize.X / 2 * (int)Alignment, mTextSize.Y / 2);
        }

        public override void Draw(GameScreen scrn, GameTime gameTime, byte alpha)
        {
            mDimColor.A = alpha;
            mTextColor.A = alpha;

            var outputRect = mRectangle;
            outputRect.Height += (int)Padding.Y * 2;
            outputRect.Width += (int)Padding.X * 2;
            
            Vector2 textPosition = new Vector2(mRectangle.X + mPaddingX + (mRectangle.Width / 2) * (int)Alignment, outputRect.Center.Y);

            scrn.ScreenManager.SpriteBatch.Draw(mBackground, outputRect, mSource, mDimColor);
            scrn.ScreenManager.SpriteBatch.DrawString(scrn.ScreenManager.Font, Text, textPosition, mTextColor, 0, mOrigin, 1, SpriteEffects.None, 1.0f);       
        }

        public void Widen(float width)
        {
            mRectangle.Width += (int)width;
            Refresh = true;
        }
    }
}

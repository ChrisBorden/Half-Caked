using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Half_Caked
{
    class Label : UIElement
    {
        private SpriteFont mFont;
        public Color ForegroundColor = DEFAULT_FOREGROUND, 
                     SelectedColor   = DEFAULT_SELECTED, 
                     InactiveColor   = DEFAULT_INACTIVE;
        private Color mTextColor;

        public static Color DEFAULT_FOREGROUND = Color.White;
        public static Color DEFAULT_SELECTED = Color.Yellow;
        public static Color DEFAULT_INACTIVE = Color.Gray;


        private string mText;
        public string Text
        {
            get { return mText; }
            set
            {
                mText = value;
        
                //Size = mFont.MeasureString(mText);

                mOrigin = new Vector2(mRectangle.Width / 2 * (int)Alignment, mRectangle.Height / 2);
                Refresh = true;
            }
        }

        public override UIState State
        {
            get { return mState; }
            set
            {
                mState = value;
                switch (mState)
                {
                    case UIState.Selected:
                        mTextColor = SelectedColor;
                        break;
                    case UIState.Inactive:
                        mTextColor = InactiveColor;
                        break;
                    default:
                        mTextColor = ForegroundColor;
                        break;
                }
            }
        }

        public Label(string txt)
            : this(txt, 10, 5, Alignment.Centered)
        {
        }

        public Label(string txt, int xPad, int yPad, Alignment algn)
        {
            mText = txt;
            mPaddingX = xPad;
            mPaddingY = yPad;
            mRectangle = new Rectangle(0, 0, 0, 0);
            Alignment = algn;
            State = UIState.Active;
        }

        public override void LoadContent(GameScreen scrn)
        {
            mFont = scrn.ScreenManager.Font;
            Size = mFont.MeasureString(mText);
            mOrigin = new Vector2(mRectangle.Width / 2 * (int)Alignment, mRectangle.Height / 2);
        }

        public override void Draw(GameScreen scrn, GameTime gameTime, byte alpha)
        {
            mTextColor.A = alpha;

            Vector2 textPosition = new Vector2(mRectangle.X + mPaddingX + (mRectangle.Width / 2) * (int)Alignment, mRectangle.Y);

            scrn.ScreenManager.SpriteBatch.DrawString(mFont, Text, textPosition, mTextColor, 0, mOrigin, 1, SpriteEffects.None, 1.0f);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Half_Caked
{
    public enum UIState
    {
        Selected,
        Active,
        Inactive
    }

    public enum Alignment
    {
        Left = 0,
        Centered = 1,
        Right = 2
    }

    abstract class UIElement
    {       
        #region Events

        /// <summary>
        /// Event raised when the button is pressed.
        /// </summary>
        public event EventHandler<PlayerIndexEventArgs> Pressed, Focused;
        
        /// <summary>
        /// Method for raising the Pressed event.
        /// </summary>
        public virtual void OnPressedElement(PlayerIndex playerIndex, int direction)
        {
            if (Pressed != null)
                Pressed(this, new PlayerIndexEventArgs(playerIndex));
        }

        /// <summary>
        /// Method for raising theFocused event.
        /// </summary>
        public virtual void OnFocusedElement(PlayerIndex playerIndex, int direction)
        {
            if (Focused != null)
                Focused(this, new PlayerIndexEventArgs(playerIndex));
        }

        #endregion

        protected SpriteFont UIFont;

        protected UIState mState = UIState.Active;
        public abstract UIState State { get; set; }

        public Alignment Alignment = Alignment.Centered;
        public bool Refresh = true, ChangesValue = false;

        protected int mLabelWidth = 0;
        public virtual int LabelWidth
        {
            get { return mLabelWidth; }
            set { mLabelWidth = value; }
        }

        protected Rectangle mRectangle;
        public virtual Vector2 Position
        {
            get { return new Vector2(mRectangle.X, mRectangle.Y); }
            set { mRectangle.X = (int)value.X; mRectangle.Y = (int)value.Y; }
        }

        public Vector2 Size
        {
            get { return new Vector2(mRectangle.Width + 2 * mPaddingX, mRectangle.Height + 2 * mPaddingY); }
            set { mRectangle.Width = (int)value.X; mRectangle.Height = (int)value.Y; }
        }

        public Vector2 Padding
        {
            get { return new Vector2(mPaddingX, mPaddingY); }
            set { mPaddingX = (int)value.X; mPaddingY = (int)value.Y; }
        }

        protected Vector2 mOrigin;
        protected float mPaddingX, mPaddingY;
        
        public virtual bool HandleMouseInput(InputState input)
        {
            var paintedArea = mRectangle;
            paintedArea.Y -= (int)mOrigin.Y;
            paintedArea.X += (int)Padding.X;
            if (paintedArea.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y) && input.IsNewMouseState() && State != UIState.Inactive)
            {
                if (input.IsNewRightMouseClick() || input.IsNewLeftMouseClick())
                    OnPressedElement(PlayerIndex.One, 0);
                return true;
            }

            return false;
        }

        public abstract void Draw(GameScreen scrn, GameTime gameTime, byte alpha);
        public void Draw(GameScreen scrn, GameTime gameTime) { Draw(scrn, gameTime, 255); }
        public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime) { }

        public virtual void LoadContent(GameScreen scrn) 
        {
            UIFont = scrn.ScreenManager.Game.Content.Load<SpriteFont>("Fonts\\menufont");
        }
    }
}

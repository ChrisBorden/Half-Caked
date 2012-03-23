#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Half_Caked
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    class Slider : UIElement
    {
        #region Fields

        MenuEntry mainLabel;
        Label valueLabel;

        /// <summary>
        /// The text rendered for this entry.
        /// </summary>
        string text;

        /// <summary>
        /// The currently selected choice.
        /// </summary>
        int value;

        Texture2D arrow, slider, thumb;

        Vector2 leftArrowPosition, rightArrowPosition, sliderPosition, thumbPosition;

        bool isDragging = false;

        #endregion

        #region Properties

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public override Vector2 Position
        {
            get { return new Vector2(mRectangle.X, mRectangle.Y); }
            set 
            {
                mRectangle.X = (int)value.X; 
                mRectangle.Y = (int)value.Y;

                mainLabel.Position = value;
                leftArrowPosition = value + new Vector2(LabelWidth, 0);

                sliderPosition = leftArrowPosition + new Vector2(28, 0);
                rightArrowPosition = sliderPosition + new Vector2(211, 0); //5 pixel padding left/right

                thumbPosition  = sliderPosition;
                thumbPosition.X += this.value * 2;

                valueLabel.Position = rightArrowPosition + new Vector2(20, 0);
            }
        }

        public override int LabelWidth
        {
            get
            {
                return (int)Math.Max(base.LabelWidth, mainLabel.Size.X);
            }
            set
            {
                base.LabelWidth = value;

                Size = valueLabel.Size + new Vector2(256 + value, 0);
                mRectangle.Width += mLabelWidth;
                mRectangle.Height = (int)mainLabel.Size.Y;
                mOrigin = Size / 2;

                Position = Position;
            }
        }

        public override UIState State
        {
            get
            {
                return mState;
            }
            set
            {
                mState = value;
                mainLabel.State = value;
                valueLabel.State = value;
            }
        }
        
        #endregion

        #region Events

        public override bool HandleMouseInput(InputState input)
        {
            if (State == UIState.Inactive)
                return false;
            
            int before = value;
            isDragging &= input.CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                          || input.CurrentMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed; 

            var rect1 = new Rectangle((int)leftArrowPosition.X, (int)leftArrowPosition.Y - 10, 20, 20);
            var rect2 = new Rectangle((int)rightArrowPosition.X, (int)rightArrowPosition.Y - 10, 20, 20);

            bool click_in_slider = rect1.Right + 5 < input.CurrentMouseState.X && rect2.Left - 5 > input.CurrentMouseState.X &&
                                   rect1.Top < input.CurrentMouseState.Y && rect1.Bottom > input.CurrentMouseState.Y;

            if (input.IsNewLeftMouseClick() || input.IsNewRightMouseClick())
            {
                if (rect1.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y))
                    value -= 5;
                else if (rect2.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y))
                    value += 5;
                else if (click_in_slider )
                    value = (int)(input.CurrentMouseState.X - (rect1.Right + 5)) / 2;
            }
            else if (isDragging)
                value = (int)(input.CurrentMouseState.X - (rect1.Right + 5)) / 2;
            else if ((input.IsNewLeftMousePress() || input.IsNewRightMousePress()) && click_in_slider)
            {
                value = (int)(input.CurrentMouseState.X - (rect1.Right + 5)) / 2;
                isDragging = true;
            }
            else
                return base.HandleMouseInput(input);

            OnPressedElement(PlayerIndex.One, 0);
            return true;
        }

        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        public override void OnPressedElement(PlayerIndex playerIndex,  int direction)
        {
            value += direction * 5;

            value = (int)MathHelper.Clamp(value, 0, 100);
            valueLabel.Text = value + "%";

            thumbPosition = sliderPosition;
            thumbPosition.X += this.value * 2;

            base.OnPressedElement(playerIndex, direction);
        }

        #endregion

        #region Initialization
        
        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public Slider(string text, int value)
        {
            ChangesValue = true;
            this.text = text;
            this.value = value;

            mainLabel = new MenuEntry(text);
            valueLabel = new Label(value + "% ");
        }

        public override void LoadContent(GameScreen screen)
        {
            mainLabel.LoadContent(screen);
            valueLabel.LoadContent(screen);

            arrow = screen.ScreenManager.Game.Content.Load<Texture2D>("UI\\arrow");
            thumb = screen.ScreenManager.Game.Content.Load<Texture2D>("UI\\thumb");
            slider = screen.ScreenManager.Game.Content.Load<Texture2D>("UI\\slider");

            mLabelWidth = (int)mainLabel.Size.X;
            mOrigin = Size / 2;
        }

        #endregion

        #region Update and Draw

        public override void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            mainLabel.Update(screen, isSelected, gameTime);
        }

        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public override void Draw(GameScreen screen, GameTime gameTime, byte b)
        {
            mainLabel.Draw(screen, gameTime, b);
            valueLabel.Draw(screen, gameTime, b);

            Color textureColor = (State == UIState.Selected) ? Color.White : Color.Gray;

            screen.ScreenManager.SpriteBatch.Draw(arrow, leftArrowPosition, arrow.Bounds, textureColor, 0, Vector2.UnitY * 10, 1f, SpriteEffects.None, 1);
            screen.ScreenManager.SpriteBatch.Draw(arrow, rightArrowPosition, arrow.Bounds, textureColor, 0, Vector2.UnitY * 10, 1f, SpriteEffects.FlipHorizontally, 1);
            screen.ScreenManager.SpriteBatch.Draw(slider, sliderPosition, slider.Bounds, textureColor, 0, Vector2.UnitY * 5f, new Vector2(2, 1.5f), SpriteEffects.None, 1);
            screen.ScreenManager.SpriteBatch.Draw(thumb, thumbPosition, thumb.Bounds, textureColor, 0, new Vector2(2.5f, 7.5f), new Vector2(2, 1.5f), SpriteEffects.None, 1);
        }

        #endregion
    }
}

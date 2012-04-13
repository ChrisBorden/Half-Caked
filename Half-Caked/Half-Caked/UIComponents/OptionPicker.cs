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
    class OptionPicker : UIElement
    {
        #region Fields

        MenuEntry mainLabel;
        Label choiceLabel;

        /// <summary>
        /// The text rendered for this entry.
        /// </summary>
        string text;

        /// <summary>
        /// The different choices that can be toggled between for the options.
        /// </summary>
        string[] choices;

        /// <summary>
        /// The currently selected choice.
        /// </summary>
        int selected;

        Texture2D arrow;

        Vector2 leftArrowPosition, rightArrowPosition, maxChoiceSize;

        #endregion

        #region Properties

        public int SelectedChoice
        {
            get { return selected; }
            set { selected = value; choiceLabel.Text = choices[value]; }
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

                choiceLabel.Position = leftArrowPosition + new Vector2(20, 0);
                rightArrowPosition = choiceLabel.Position + maxChoiceSize + new Vector2(choiceLabel.Padding.X * 2, 0);
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
                choiceLabel.State = value;
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

                Size = maxChoiceSize + new Vector2(40 + choiceLabel.Padding.X * 2, 0);
                mRectangle.Width += mLabelWidth;
                mRectangle.Height += (int)mainLabel.Size.Y;

                Position = Position;
            }
        }
        
        #endregion

        #region Events

        public override bool HandleMouseInput(InputState input)
        {
            int direction = 0;

            if(input.IsNewLeftMouseClick() || input.IsNewRightMouseClick())
            {
                var rect1 = new Rectangle((int)leftArrowPosition.X, (int)leftArrowPosition.Y - 10, 20, 20);
                var rect2 = new Rectangle((int)rightArrowPosition.X, (int)rightArrowPosition.Y - 10, 20, 20);
                var rect3 = new Rectangle(rect1.Right, rect1.Top, (int)(maxChoiceSize.X + choiceLabel.Padding.X * 2), mRectangle.Height);

                if (rect1.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y))
                    direction = -1;
                else if (rect2.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y))
                    direction = 1;
                else if (rect3.Contains(input.CurrentMouseState.X, input.CurrentMouseState.Y))
                    direction = input.IsNewRightMouseClick() ? -1 : 1;

                if (direction != 0)
                {
                    OnPressedElement(PlayerIndex.One, direction);
                    return true;
                }
            }

            return base.HandleMouseInput(input);
        }

        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        public override void OnPressedElement(PlayerIndex playerIndex, int direction)
        {
            this.selected = (this.selected + direction + choices.Length) % choices.Length;
            choiceLabel.Text = choices[selected];
            base.OnPressedElement(playerIndex, direction);
        }

        #endregion

        #region Initialization
        
        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public OptionPicker(string text, string[] choices)
        {
            ChangesValue = true;

            this.text = text;
            this.choices = choices;

            mainLabel = new MenuEntry(text);
            choiceLabel = new Label(choices[0]);
        }

        public override void LoadContent(GameScreen screen)
        {
            mainLabel.LoadContent(screen);
            choiceLabel.LoadContent(screen);

            arrow = screen.ScreenManager.Game.Content.Load<Texture2D>("UI\\arrow");
            SpriteFont font = screen.ScreenManager.Game.Content.Load<SpriteFont>("Fonts\\menufont");
            maxChoiceSize = Vector2.Zero;

            for(int i = 0; i < choices.Length; i++)
            {
                Vector2 temp = font.MeasureString(choices[i]);
                maxChoiceSize.X = Math.Max(maxChoiceSize.X, temp.X);
            }

            LabelWidth = (int)mainLabel.Size.X;
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
            choiceLabel.Draw(screen, gameTime, b);

            Color arrowColor = (State == UIState.Selected) ? Color.White : Color.Gray;

            screen.ScreenManager.SpriteBatch.Draw(arrow, leftArrowPosition, arrow.Bounds, arrowColor, 0, Vector2.UnitY * 10, 1f, SpriteEffects.None, 1);
            screen.ScreenManager.SpriteBatch.Draw(arrow, rightArrowPosition, arrow.Bounds, arrowColor, 0, Vector2.UnitY * 10, 1f, SpriteEffects.FlipHorizontally, 1);
        }

        #endregion
    }
}

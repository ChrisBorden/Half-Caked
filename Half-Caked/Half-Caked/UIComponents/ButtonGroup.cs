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
    class ButtonGroup : UIElement
    {
        #region Fields

        MenuEntry mainLabel;
        Button[] buttons;

        /// <summary>
        /// The text rendered for this entry.
        /// </summary>
        string text;
        
        /// <summary>
        /// The currently selected choice.
        /// </summary>
        int selected = 0;
        
        #endregion

        #region Properties

        public bool HideInactive 
        { 
            get; 
            set; 
        }

        public MenuEntry Label
        {
            get { return mainLabel; }
        }

        public int SelectedButton
        {
            get { return selected; }
            set { selected = value; }
        }

        public Button[] Buttons
        {
            get { return buttons; }
        }

        public override Vector2 Position
        {
            get { return new Vector2(mRectangle.X, mRectangle.Y); }
            set 
            {
                mRectangle.X = (int)value.X; 
                mRectangle.Y = (int)value.Y;

                mainLabel.Position = value;
                var pos = value + new Vector2(LabelWidth + 5, 0);
                pos.Y -= buttons[0].Size.Y / 2;
                
                foreach (Button b in buttons)
                {
                    b.Position = pos;
                    pos.X += b.Size.X + 5;
                }
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

                for (int i = 0; i < buttons.Length; i++)
                    if (value == UIState.Selected)
                        buttons[i].State = (i == selected) ? value : UIState.Active;
                    else if (!HideInactive)
                        buttons[i].State = State;
                    else
                        buttons[i].State = UIState.Inactive; 
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
                RecomputeSize();
            }
        }

        private int mButtonWidth;
        public int ButtonWidth
        {
            get
            {
                return mButtonWidth;
            }
            set
            {
                mButtonWidth = value;

                foreach (Button btn in buttons)
                    btn.Widen(Math.Max(0, mButtonWidth - btn.Size.X));

                RecomputeSize();
            }
        }
        
        #endregion

        #region Events

        public override bool HandleMouseInput(InputState input)
        {
            for(int i = 0; i < buttons.Length; i++)
                if (buttons[i].HandleMouseInput(input))
                {
                    selected = i;
                    State = State;
                    return true;
                }

            return base.HandleMouseInput(input);
        }

        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        public override void OnPressedElement(PlayerIndex playerIndex, int direction)
        {
            this.selected = (int)MathHelper.Clamp(this.selected + direction, 0, buttons.Length - 1);
            State = State;

            if (direction == 0)
                buttons[selected].OnPressedElement(playerIndex, direction);

            base.OnPressedElement(playerIndex, direction);
        }

        #endregion

        #region Initialization
        
        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public ButtonGroup(string text, string[] choices)
        {
            HideInactive = false;
            ChangesValue = true;

            this.text = text;
            mainLabel = new MenuEntry(text);

            buttons = new Button[choices.Length];
            for(int i = 0; i < choices.Length; i++)
                buttons[i] = new Button(choices[i]);

            mPaddingY = 5;
        }

        public override void LoadContent(GameScreen screen)
        {
            mainLabel.LoadContent(screen);

            int btnWidth = 0;
            foreach (Button btn in buttons)
            {
                btn.LoadContent(screen);
                btnWidth = (int)Math.Max(btnWidth, btn.Size.X);
            }

            mLabelWidth = (int)mainLabel.Size.X;
            ButtonWidth = btnWidth;
            mOrigin = Size / 2;
        }

        #endregion

        #region Update and Draw

        public override void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            mainLabel.Update(screen, isSelected, gameTime);

            if (State == UIState.Selected || !HideInactive)
                foreach (Button btn in buttons)
                    btn.Update(screen, isSelected, gameTime);
        }

        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public override void Draw(GameScreen screen, GameTime gameTime, byte b)
        {
            mainLabel.Draw(screen, gameTime, b);

            if(State == UIState.Selected || !HideInactive)
                foreach (Button btn in buttons)
                    btn.Draw(screen, gameTime, b);
        }

        #endregion

        //TODO: Make mouse selection of a Button in the group actually trigger the ButtonGroup's Pressed event

        private void RecomputeSize()
        {
            Size = new Vector2(LabelWidth + ButtonWidth * buttons.Length, buttons[0].Size.Y);
            mOrigin = Size / 2;

            Position = Position;
        }
    }
}

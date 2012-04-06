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
    class StackPanel : UIElement
    {
        #region Fields

        UIElement[] mElements;
        int mSelected = 0;
        
        #endregion

        #region Properties
             
        public int SelectedButton
        {
            get { return mSelected; }
            set { mSelected = value; }
        }

        public UIElement[] Elements
        {
            get { return mElements; }
        }

        public override Vector2 Position
        {
            get { return new Vector2(mRectangle.X, mRectangle.Y); }
            set 
            {
                mRectangle.X = (int)value.X; 
                mRectangle.Y = (int)value.Y;
                RecomputeSize();
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

                for (int i = 0; i < mElements.Length; i++)
                    if (value == UIState.Selected)
                        mElements[i].State = (i == mSelected) ? value : UIState.Active;
                    else
                        mElements[i].State = value;
            }
        }
                
        #endregion

        #region Events

        public override bool HandleMouseInput(InputState input)
        {
            for(int i = 0; i < mElements.Length; i++)
                if (mElements[i].HandleMouseInput(input))
                {
                    mSelected = i;
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
            this.mSelected = (int)MathHelper.Clamp(this.mSelected + direction, 0, mElements.Length - 1);
            State = State;

            if (direction == 0)
                mElements[mSelected].OnPressedElement(playerIndex, direction);

            base.OnPressedElement(playerIndex, direction);
        }

        #endregion

        #region Initialization
        
        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public StackPanel(UIElement[] elements)
        {
            ChangesValue = true;

            mElements = elements;
            Padding = Vector2.UnitX * 5;
            mPaddingY = 5;
        }

        public override void LoadContent(GameScreen screen)
        {
            foreach (UIElement element in mElements)
                element.LoadContent(screen);

            RecomputeSize();
        }

        #endregion

        #region Update and Draw

        public override void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            int i = 0;
            foreach (UIElement element in mElements)
                element.Update(screen, isSelected && (i++ == mSelected), gameTime);
        }

        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public override void Draw(GameScreen screen, GameTime gameTime, byte b)
        {
            foreach (UIElement element in mElements)
                element.Draw(screen, gameTime, b);
        }

        #endregion

        #region Private Helper Methods

        private void RecomputeSize()
        {
            Vector2 tempSize = Vector2.Zero;
            foreach (UIElement element in Elements)
            {
                element.Position = new Vector2(Position.X + tempSize.X, Position.Y );//+ element.Size.Y/2);

                tempSize.Y = Math.Max(tempSize.Y, element.Size.Y);
                tempSize.X += element.Size.X + mPaddingX;
            }

            Size = tempSize;
            mOrigin = Size / 2;
        }

        #endregion
    }
}

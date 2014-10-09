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
#endregion

namespace Half_Caked
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    class MenuEntry : Label
    {
        #region Fields
        
        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float selectionFade;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public MenuEntry(string text)
            : base (text, 5, 2, Alignment.Left)
        {
        }

        public override void LoadContent(GameScreen screen)
        {
            Size = screen.ScreenManager.Font.MeasureString(Text) * 1.1f;
        }
        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public override void Update(MenuScreen screen, bool isSelected,
                                                      GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
            else
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
        }


        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public override void Draw(GameScreen screen, GameTime gameTime, byte b)
        {
            // Draw the selected entry in yellow, otherwise white.
            Color color = mTextColor;

            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;
            
            float pulsate = (float)Math.Sin(time * 6) + 1;
            
            float scale = 1 + pulsate * 0.05f * selectionFade;

            // Modify the alpha to fade text out during transitions.
            color = new Color(color.R, color.G, color.B, screen.TransitionAlpha);

            // Draw text, centered on the middle of each line.
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            SpriteFont font = screenManager.Font;

            Vector2 textPosition = new Vector2(mRectangle.X + mPaddingX + (mRectangle.Width / 2) * (int)Alignment, mRectangle.Y);
            mOrigin = new Vector2(mRectangle.Width / 2.2f * (int)Alignment * scale, mRectangle.Height / 2.2f * scale);

            spriteBatch.DrawString(font, Text, textPosition, color, 0,
                                   mOrigin, scale, SpriteEffects.None, 0);
        }

        #endregion
    }
}

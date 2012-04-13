#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Half_Caked
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        #region Fields

        List<UIElement> menuEntries = new List<UIElement>();
        protected int selectedEntry = 0;
        string menuTitle;
        SoundEffect EntryFocusChanged;
        Song mMenuMusic;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<UIElement> MenuEntries
        {
            get { return menuEntries; }
        }


        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            try
            {
                EntryFocusChanged = ScreenManager.Game.Content.Load<SoundEffect>("Sounds\\UISelected");
                mMenuMusic = ScreenManager.Game.Content.Load<Song>("Sounds\\MenuMusic");
            }
            catch { }

            menuEntries[selectedEntry].State = UIState.Selected;

            foreach (UIElement ui in menuEntries)
                ui.LoadContent(this);

            PositionElements();
        }

        public void PositionElements()
        {
            Vector2 position = new Vector2(100, 150);
            int maxWidth = 0;

            for (int i = 0; i < menuEntries.Count; i++)
                if (maxWidth < menuEntries[i].LabelWidth)
                    maxWidth = menuEntries[i].LabelWidth;

            for (int i = 0; i < menuEntries.Count; i++)
            {
                menuEntries[i].Position = position;
                menuEntries[i].LabelWidth = maxWidth;
                position.Y += menuEntries[i].Size.Y;
            }
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            var before = selectedEntry;

            // Move to the previous menu entry?
            if (input.IsMenuUp(ControllingPlayer))
            {
                selectedEntry--;
            }

            // Move to the next menu entry?
            if (input.IsMenuDown(ControllingPlayer))
            {
                selectedEntry++;
            }

            selectedEntry = (int)MathHelper.Clamp(selectedEntry, 0, MenuEntries.Count - 1);

            if (!menuEntries[selectedEntry].HandleMouseInput(input))
                for (int i = 0; i < menuEntries.Count; i++)
                {
                    if(menuEntries[i].HandleMouseInput(input))
                    {
                        selectedEntry = i;
                        break;
                    }
                } 

            if(selectedEntry != before)
            {
                menuEntries[before].State = UIState.Active;
                menuEntries[selectedEntry].State = UIState.Selected;

                AudioSettings settings = (ScreenManager.Game as HalfCakedGame).CurrentProfile.Audio;

                if(EntryFocusChanged != null)
                    EntryFocusChanged.Play(settings.SoundEffectsVolume / 500f, 0f, 0f);
            }

            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(selectedEntry, playerIndex, 0);
            }
            else if (menuEntries[selectedEntry].ChangesValue && input.IsNextButton(ControllingPlayer))
            {
                OnSelectEntry(selectedEntry, playerIndex, 1);
            }
            else if (menuEntries[selectedEntry].ChangesValue && (input.IsPreviousButton(ControllingPlayer)))
            {
                OnSelectEntry(selectedEntry, playerIndex, -1);
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex, int direction)
        {
            menuEntries[selectedEntry].OnPressedElement(playerIndex, direction);
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
            this.ScreenManager.Game.IsMouseVisible = false;
        }


        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }


        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool topScreen,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, topScreen, coveredByOtherScreen);

            ScreenManager.Game.IsMouseVisible = true;

            if (MediaPlayer.State == MediaState.Stopped)
                try
                {
                    MediaPlayer.Play(mMenuMusic);
                }
                catch { }

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                menuEntries[i].Update(this, isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 position = new Vector2(100, 150);

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                menuEntries[i].Position = position;
                position.Y += menuEntries[i].Size.Y;

                menuEntries[i].Draw(this, gameTime, TransitionAlpha);
            }

            // Draw the menu title.
            Vector2 titlePosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width/2, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192, TransitionAlpha);
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion
    }
}

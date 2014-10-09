#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System.Speech.Synthesis;
using System.Linq;
#endregion

namespace Half_Caked
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class PauseMenuScreen : MenuScreen
    {
        #region Initialization
        private Level mLevel;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen(Level level)
            : base("Paused")
        {
            // Flag that there is no need for the game to transition
            // off when the pause menu is on top of it.
            IsPopup = true;
            mLevel = level;

            // Create our menu entries.
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume Game");
            MenuEntry restartLevelMenuEntry = new MenuEntry("Restart Level");
            MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");
            
            // Hook up menu event handlers.
            resumeGameMenuEntry.Pressed += OnCancel;
            quitGameMenuEntry.Pressed += QuitGameMenuEntrySelected;
            restartLevelMenuEntry.Pressed += RestartLevelMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(restartLevelMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to quit this level?\n     All current progress will be lost.";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Buttons[0].Pressed += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }

        void RestartLevelMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to restart this level?\n     All current progress will be lost.";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Buttons[0].Pressed += ConfirmResetMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);          
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
            Microsoft.Xna.Framework.Media.MediaPlayer.Stop();
        }

        void ConfirmResetMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.GetScreens().OfType<GameplayScreen>().First().Narrator.SpeakAsyncCancelAll();
            mLevel.Reset();
            OnCancel(sender, e);
        }

        #endregion

        #region Draw


        /// <summary>
        /// Draws the pause menu screen. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base MenuScreen.Draw.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            var viewport = ScreenManager.GraphicsDevice.Viewport;
            float scale = MathHelper.Min((viewport.Width - 550f) / mLevel.Size.Width, (viewport.Height - 290f) / mLevel.Size.Height); 

            if (ScreenState != Half_Caked.ScreenState.TransitionOff && ScreenState != Half_Caked.ScreenState.Hidden && scale >= .05f)
            {
                ScreenManager.SpriteBatch.Begin();
                mLevel.DrawMap(ScreenManager.SpriteBatch, gameTime, new Vector2(400, 140), scale );
                ScreenManager.SpriteBatch.End();
            }

            base.Draw(gameTime);
        }


        #endregion
    }
}

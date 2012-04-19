#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Speech.Synthesis; 
#endregion

namespace Half_Caked
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Level mLevel;
        InputState mInputState;
        bool mIsBound = false;
        Rectangle mOldClip;

        #endregion

        public SpeechSynthesizer Narrator;

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(int levelNumber)
        {
            Narrator = new SpeechSynthesizer();
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            mLevel = Level.LoadLevel(levelNumber);
            Microsoft.Xna.Framework.Media.MediaPlayer.Stop();
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            mLevel.LoadContent(this.content, (ScreenManager.Game as HalfCakedGame).CurrentProfile);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        #endregion

        #region Update and Draw

        [DllImport("user32.dll")]
        static extern void ClipCursor(ref Rectangle rect);

        [DllImport("user32.dll")]
        static extern void GetClipCursor(ref Rectangle rect);

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool topScreen,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, topScreen, coveredByOtherScreen);

            if (IsActive)
            {
                if (Narrator.State == SynthesizerState.Paused)
                    Narrator.Resume();

                ScreenManager.Game.IsMouseVisible = true;
                // Prevent mouse cursor from leaving window when in game.
                if (!mIsBound)
                {
                    GetClipCursor(ref mOldClip);
                    Form.FromHandle(this.ScreenManager.Game.Window.Handle).Cursor = ScreenManager.GameCursor;

                    Rectangle rect = this.ScreenManager.Game.Window.ClientBounds;
                    rect.Width += rect.X;
                    rect.Height += rect.Y;
                    ClipCursor(ref rect);
                    mIsBound = true;
                }

                if(mInputState == null)
                    return;

                try
                {
                    mLevel.Update(gameTime, this.ScreenManager, mInputState);
                }
                catch (Exception E)
                {
                    if (E.Message.Equals("LevelComplete"))
                        ScreenManager.AddScreen(new LevelOverScreen(mLevel, (ScreenManager.Game as HalfCakedGame)), ControllingPlayer);
                    else
                        throw E;
                }
            }
            else if (!ScreenManager.Game.IsActive && topScreen)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(mLevel), ControllingPlayer);
            }
            else
            {
                if (Narrator.State == SynthesizerState.Speaking)
                    Narrator.Pause();

                ClipCursor(ref mOldClip);

                Form.FromHandle(this.ScreenManager.Game.Window.Handle).Cursor = ScreenManager.DefaultCursor;
                mIsBound = false;
            }

        }

        public override void ExitScreen()
        {
            if (Narrator.State != SynthesizerState.Ready)
                Narrator.SpeakAsyncCancelAll();

            base.ExitScreen();
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            mInputState = input;
            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPausingGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(mLevel), ControllingPlayer);
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();
            mLevel.Draw(spriteBatch, gameTime);
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }


        #endregion
    }
}

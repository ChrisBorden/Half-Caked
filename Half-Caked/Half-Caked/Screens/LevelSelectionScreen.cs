#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System.Threading;
#endregion

namespace Half_Caked
{
    class LevelSelectionScreen : MenuScreen
    {
        #region Private Fields
        Level mCurLevel;
        Thread levelFinder;
        #endregion

        #region Initialization

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public LevelSelectionScreen(Profile p)
            : base("Level Selection")
        {
            for (int i = 1; i <= p.CurrentLevel + 1; i++)
            {
                MenuEntry entry = new MenuEntry("Level " + i);
                entry.Pressed += EntrySelected;
                entry.Focused += EntryFocused;
                MenuEntries.Add(entry);
            }
            EntryFocused(MenuEntries[0], new PlayerIndexEventArgs(PlayerIndex.One));
        }


        #endregion

        #region Handle Input
        
        /// <summary>
        /// Event handler for when a level is selected
        /// </summary>
        void EntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen(MenuEntries.IndexOf(sender as MenuEntry)));
        }

        /// <summary>
        /// Event handler for when a level is focused
        /// </summary>
        void EntryFocused(object sender, PlayerIndexEventArgs e)
        {
            if(levelFinder != null)
                levelFinder.Abort();
            mCurLevel = null;

            ThreadStart ts = delegate() 
            { 
                var lvl = Level.LoadLevel(this.selectedEntry);
                lvl.LoadContent(ScreenManager.Game.Content, (ScreenManager.Game as HalfCakedGame).CurrentProfile);
                mCurLevel = lvl;
            };
            levelFinder = new Thread(ts);
            levelFinder.Start();
        }
        
        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (mCurLevel != null)
            {
                var viewport = ScreenManager.GraphicsDevice.Viewport;
                float scale = MathHelper.Min(viewport.Width / 2f / mCurLevel.Size.Width, (viewport.Height - 350f) / mCurLevel.Size.Height);

                if (ScreenState != Half_Caked.ScreenState.TransitionOff && ScreenState != Half_Caked.ScreenState.Hidden && scale >= .05f)
                {
                    ScreenManager.SpriteBatch.Begin();
                    mCurLevel.DrawMap(ScreenManager.SpriteBatch, gameTime, new Vector2(viewport.Width / 2 - 150, 140), scale);
                    
                    Statistics lvlStats = (ScreenManager.Game as HalfCakedGame).CurrentProfile.LevelStatistics[selectedEntry];

                    var textpos = new Vector2(viewport.Width / 2 - 150, mCurLevel.Size.Height * scale + 170);
                    string text = "You haven't beaten this level yet!";
                    if (lvlStats != null)
                    {
                        text = "High Score:\nPortals Used:\nDeaths\nTime:";

                        var otherText = lvlStats.Score + "\n" + lvlStats.PortalsOpened +
                                        "\n" + lvlStats.Deaths + "\n" + (int)lvlStats.TimeElapsed;

                        var size = ScreenManager.Font.MeasureString(text);
                        size.Y = 0;
                        size.X += 10;

                        ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, otherText, textpos + size, Color.White);   
                    }

                    ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, text, textpos, Color.White);      
                    ScreenManager.SpriteBatch.End();
                }

                
            }
        }

        #endregion
    }
}

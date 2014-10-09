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
using System.IO;
using System.Collections.Generic;
#endregion

namespace Half_Caked
{
    class CustomLevelSelectionScreen : MenuScreen
    {
        #region Private Fields
        List<Level> mLevels = new List<Level>();
        #endregion

        #region Initialization

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public CustomLevelSelectionScreen(Profile p)
            : base("Custom Level Selection")
        {
            foreach(string s in Directory.GetFiles(@"Content\Levels\Custom\", "*.xml", SearchOption.AllDirectories))
            {
                Level curLevel = Level.LoadLevel(s);
                mLevels.Add(curLevel);
                MenuEntry entry = new MenuEntry(curLevel.Name);

                entry.Pressed += EntrySelected;
                MenuEntries.Add(entry);
            }

            MenuEntry backButton = new MenuEntry("Back");
            backButton.Pressed += OnCancel;
            MenuEntries.Add(backButton);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            ThreadStart ts = delegate()
            {
                foreach (Level lvl in mLevels)
                    lvl.LoadContent(ScreenManager, (ScreenManager.Game as HalfCakedGame).CurrentProfile);
            };
            new Thread(ts).Start();
        }

        #endregion

        #region Handle Input
        
        /// <summary>
        /// Event handler for when a level is selected
        /// </summary>
        void EntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen(mLevels[selectedEntry]));
        }
        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.selectedEntry < mLevels.Count && mLevels[this.selectedEntry].IsLoaded)
            {
                var mCurLevel = mLevels[selectedEntry];

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

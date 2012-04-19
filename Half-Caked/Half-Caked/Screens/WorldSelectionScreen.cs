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
    class WorldSelectionScreen : MenuScreen
    {
        #region Initialization

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public WorldSelectionScreen(Profile p)
            : base("World Selection")
        {
            for (int i = 0; Level.INIT_LID_FOR_WORLD[i] <= p.CurrentLevel && i < Level.WORLD_NAMES.Length; i++)
            {
                MenuEntry entry = new MenuEntry(Level.WORLD_NAMES[i]);
                entry.Pressed += EntrySelected;
                MenuEntries.Add(entry);
            }
            
            MenuEntry cstmButton = new MenuEntry("Custom Levels");
            cstmButton.Pressed += CustomEntrySelected;
            MenuEntries.Add(cstmButton);

            MenuEntry backButton = new MenuEntry("Back");
            backButton.Pressed += OnCancel;
            MenuEntries.Add(backButton);
        }


        #endregion

        #region Handle Input
        
        /// <summary>
        /// Event handler for when a level is selected
        /// </summary>
        void EntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new LevelSelectionScreen((ScreenManager.Game as HalfCakedGame).CurrentProfile, selectedEntry), e.PlayerIndex);
        }

        void CustomEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new CustomLevelSelectionScreen((ScreenManager.Game as HalfCakedGame).CurrentProfile), e.PlayerIndex);
        }
        
        #endregion

 /*       #region Draw

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (mLoaded[this.selectedEntry])
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

        #endregion*/
    }
}

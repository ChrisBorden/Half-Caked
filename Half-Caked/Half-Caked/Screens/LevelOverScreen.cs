using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Half_Caked
{
    class LevelOverScreen : ContentBoxScreen
    {
        Level mLevel;

        public LevelOverScreen(Level level, HalfCakedGame game)
            : base("Level Complete!", "", new string[] {"Next Level", "Play Again", "Exit"}, 0)
        {
            IsPopup = true;
            mLevel = level;
            
            // Hook up menu event handlers.
            Buttons[0].Pressed += NextLevelMenuEntrySelected;
            Buttons[1].Pressed += ResetMenuEntrySelected;
            Buttons[2].Pressed += QuitGameMenuEntrySelected;
            Cancelled += QuitGameMenuEntrySelected;

            Profile prof = game.CurrentProfile;

            if (level.LevelIdentifier != -1) //One of the default levels
            {
                if (prof.CurrentLevel < Level.INIT_LID_FOR_WORLD.Last() && prof.CurrentLevel == level.LevelIdentifier)
                    prof.CurrentLevel++;

                if (prof.LevelStatistics[mLevel.LevelIdentifier] == null ||
                    prof.LevelStatistics[mLevel.LevelIdentifier].Score < level.LevelStatistics.Score)
                {
                    mContentLabel.Text = "Congratulations! You set a new High Score: " + level.LevelStatistics.Score;
                    level.LevelStatistics.Date = DateTime.Now;
                    prof.LevelStatistics[mLevel.LevelIdentifier] = level.LevelStatistics;

                    if (prof.GlobalIdentifer != -1)
                        level.LevelStatistics.UploadScore(prof.GlobalIdentifer);
                    else
                        prof.Register();

                    Profile.SaveProfile(prof, "default.sav", game.Device);
                }
                else
                {
                    mContentLabel.Text = "High Score: " + prof.LevelStatistics[mLevel.LevelIdentifier].Score
                                          + "   |   Your Score: " + level.LevelStatistics.Score;
                }
            }
            else //Custom level code
            {
                var entry = prof.CustomLevelStatistics.Find(x => x.Key == level.CustomLevelIdentifier); 
                if (entry == null || entry.Value.Score < level.LevelStatistics.Score)
                {
                    mContentLabel.Text = "Congratulations! You set a new High Score: " + level.LevelStatistics.Score;
                    level.LevelStatistics.Date = DateTime.Now;

                    if(entry != null)
                        entry.Value = level.LevelStatistics;
                    else
                        prof.CustomLevelStatistics.Add(new KeyValuePair<Guid,Statistics>(level.CustomLevelIdentifier, level.LevelStatistics));                    
                    
                    Profile.SaveProfile(prof, "default.sav", game.Device);
                }
                else
                {
                    mContentLabel.Text = "High Score: " + entry.Value.Score + "   |   Your Score: " + level.LevelStatistics.Score;
                }
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (mLevel.LevelIdentifier + 1 >= Level.INIT_LID_FOR_WORLD.Last() || mLevel.LevelIdentifier == -1)
            {
                Buttons[0].State = UIState.Inactive;
                Buttons[1].State = UIState.Selected;
                mSelectedButton = 1;
            }
        }

        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }

        void NextLevelMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen(Level.LoadLevel(mLevel.LevelIdentifier + 1)));
        }

        void ResetMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            mLevel.Reset();
            ExitScreen();
        }
        
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}

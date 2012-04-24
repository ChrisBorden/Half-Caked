using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Half_Caked
{
    public class Level : Sprite
    {
        #region Constants
        public static float METERS_TO_UNITS = 20;

        public static int[] INIT_LID_FOR_WORLD = { 0, 8 };
        public static string[] WORLD_NAMES = { "Training Grounds";

        private const float LONG_DIST = .4f;
        private const float SHORT_DIST = .01f;
        #endregion

        #region Fields
        public float Gravity { get; set; }
        public Guid CustomLevelIdentifier { get; set; }
        public string Name { get; set; }

        private int mLevelID = -1;
        public int LevelIdentifier
        {
            get { return mLevelID; }
            set { mLevelID = value; if (LevelStatistics != null) LevelStatistics.Level = value; }
        }

        [XmlIgnore]
        public Statistics LevelStatistics;

        public List<Tile> Tiles;
        public List<Obstacle> Obstacles;
        public List<Actor> Actors;
        public List<Checkpoint> Checkpoints;

        [XmlIgnore]
        public PortalGroup Portals;
        [XmlIgnore]
        public Character Player;

        private Vector2 mDimensions;
        private Vector2 mCenterVector;
        private int mCheckpointIndex = 0;

        private List<TextEffect> mTextEffects;
        private AudioSettings mAudio;
        private SoundEffect mExitReached;
        private Song mBackgroundMusic;
        private SoundEffect mCheckpointSound;
        private bool mCanPlayerMusic = true;

        private Sprite mBackground;        
        private SpriteFont mGameFont;
        private Sprite mCakeSprite;

        private bool mLoaded = false;
        public bool IsLoaded
        {
            get { return mLoaded; }
        }

        #endregion

        #region Initialization
        public Level()
        {
            Gravity = 40f;
            Name = "New Level";

            LevelStatistics = new Statistics();
            mBackground = new Sprite();
            mCakeSprite = new Sprite();
            Obstacles = new List<Obstacle>();
            Actors = new List<Actor>();
            Checkpoints = new List<Checkpoint>();
            mTextEffects = new List<TextEffect>();
            Tiles = new List<Tile>();
            Portals = new PortalGroup();
        }

        public void LoadContent(ScreenManager screenManager, Profile activeProfile)
        {
            if (mLoaded)
                return;

            var theContentManager = screenManager.Game.Content;

            string backgroundMusicName = "Sounds\\" + AssetName;

            if (LevelIdentifier == -1)
            {
                string filepath = "Content\\Levels\\Custom\\" + AssetName;
                base.LoadContent(screenManager.GraphicsDevice, filepath + ".png");

                try
                {
                    mBackground.LoadContent(screenManager.GraphicsDevice, filepath + "b.png");
                    mBackground.Position = Position;
                }
                catch
                {
                    mBackground = null;
                }
            }
            else
            {
                string filepath = "Levels\\" + AssetName;
                base.LoadContent(theContentManager, filepath);

                try
                {
                    mBackground.LoadContent(theContentManager, filepath + "b");
                    mBackground.Position = Position;
                }
                catch
                {
                    mBackground = null;
                }
            }

            mCakeSprite.LoadContent(theContentManager, "Sprites\\Cake");
            mCakeSprite.Scale = .25f;
            mCakeSprite.Position = Checkpoints[Checkpoints.Count - 1].Location - Vector2.UnitY * mCakeSprite.Size.Height;

            mDimensions = activeProfile.Graphics.Resolution;
            mCenterVector = new Vector2(mDimensions.X / 2 - 100, mDimensions.Y * 3 / 4 - 100);

            mAudio = activeProfile.Audio;
            SoundEffect.MasterVolume = mAudio.MasterVolume / 100f;
            MediaPlayer.Volume = mAudio.MasterVolume * mAudio.MusicVolume / 10000f;

            mExitReached = theContentManager.Load<SoundEffect>("Sounds\\ExitReached");
			try
			{
				mBackgroundMusic = theContentManager.Load<Song>(backgroundMusicName);
			}
			catch
			{
				mBackgroundMusic = theContentManager.Load<Song>("Sounds\\Level");
			}
            mCheckpointSound = theContentManager.Load<SoundEffect>("Sounds\\Checkpoint");

            Portals.LoadContent(theContentManager);
            
            Player = new Character();
            Player.LoadContent(theContentManager);
            Player.Position = Player.InitialPosition = Checkpoints[0].Location;

            foreach (Obstacle spr in Obstacles)
                spr.LoadContent(theContentManager, spr.AssetName);

            foreach (Actor spr in Actors)
                spr.LoadContent(theContentManager, spr.AssetName);

            mGameFont = theContentManager.Load<SpriteFont>("Fonts\\gamefont");

            mLoaded = true;
        }
        #endregion

        #region Update and Draw
        public void Update(GameTime theGameTime, ScreenManager manager, InputState inputState)
        {
            if (MediaPlayer.State == MediaState.Stopped && mCanPlayerMusic)
                try
                {
                    MediaPlayer.Play(mBackgroundMusic);
                }
                catch { mCanPlayerMusic = false;  }

            KeyboardState aCurrentKeyboardState = Keyboard.GetState();
            MouseState aCurrentMouseState = Mouse.GetState();
            LevelStatistics.TimeElapsed += theGameTime.ElapsedGameTime.TotalSeconds;

            Portals.ClearSprites();
            
            Player.Update(theGameTime, this, inputState);

            foreach (Obstacle spr in Obstacles)
                spr.Update(theGameTime);

            foreach (Actor spr in Actors)
            {
                spr.Update(theGameTime, this);
            }

            mTextEffects = mTextEffects.Where(x => !x.Done).ToList();
            foreach (TextEffect effect in mTextEffects)
            {
                effect.Update(theGameTime);
            }

            Portals.Update(theGameTime);
                        
            if(Player.IsGrounded())
                while (Checkpoints[mCheckpointIndex].InBounds(Player.Position))
                {
                    if (++mCheckpointIndex >= Checkpoints.Count)
                    {
                        GameOver();
                    }
                    else
                    {
                        mTextEffects.Add(new CheckpointNotification(Player.Position+Position));
                        if (Checkpoints[mCheckpointIndex - 1].NarrationText != null && Checkpoints[mCheckpointIndex - 1].NarrationText.Length > 0)
                            mTextEffects.Add(new NarrationEffect(Checkpoints[mCheckpointIndex - 1].NarrationText, manager));

                        PlaySoundEffect(mCheckpointSound);
                    }
                }
        }

        public override void Draw(SpriteBatch theSpriteBatch, GameTime theGameTime)
        {
            Vector2 offset = new Vector2(MathHelper.Clamp((mCenterVector - Player.Position).X, mDimensions.X - Size.Width, 0), MathHelper.Clamp((mCenterVector - Player.Position).Y, mDimensions.Y - Size.Height, 0)) - Position;
            float dist = offset.Length();// Vector2.Multiply(offset, new Vector2(1, this.Size.Height / (float)Size.Width)).Length();

            if (dist > LONG_DIST * mDimensions.X || dist < SHORT_DIST * mDimensions.X)
            {
                Position += offset;
                Velocity = Vector2.Zero;
                Acceleration = Vector2.Zero;
            }
            else
            {
                offset.Normalize();
                Velocity = (Velocity.Length() + 10) * offset;
                base.Update(theGameTime);
            }

            if (mBackground != null)
            {
                mBackground.Position = Position;
                mBackground.Draw(theSpriteBatch, theGameTime);
            }

            foreach (Obstacle spr in Obstacles)
                spr.Draw(theSpriteBatch, Position);

            foreach (Sprite spr in Actors)
                spr.Draw(theSpriteBatch, Position);

            //This draws the animated parts of the player
            Player.AnimatedDraw(theSpriteBatch, Position, theGameTime);

            //This draws non-animated parts of the player
            Player.Draw(theSpriteBatch, Position);

            Portals.Draw(theSpriteBatch, Position);

            mCakeSprite.Draw(theSpriteBatch, Position);
            base.Draw(theSpriteBatch, theGameTime);
            Portals.DrawPortals(theSpriteBatch, Position);

            foreach (TextEffect effect in mTextEffects)
                effect.Draw(theSpriteBatch, mGameFont);
        }

        public void DrawMap(SpriteBatch theSpriteBatch, GameTime theGameTime, Vector2 offset, float scale)
        {
            if(mBackground != null)
                mBackground.Draw(theSpriteBatch, offset - Position*scale, scale);
            foreach (Obstacle spr in Obstacles)
                spr.Draw(theSpriteBatch, offset, scale);

            foreach (Sprite spr in Actors)
                spr.Draw(theSpriteBatch, offset, scale);

            //This draws non-animated parts of the player
            Player.Draw(theSpriteBatch, offset, scale);
            mCakeSprite.Draw(theSpriteBatch, offset, scale);

            base.Draw(theSpriteBatch, offset - Position * scale, scale);
        }

        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            Portals.Reset();
            LevelStatistics = new Statistics(mLevelID);

            if(mBackground != null)
                mBackground.Position = Position;

            foreach (Obstacle spr in Obstacles)
                spr.Reset();

            Player.Reset();

            foreach (Actor spr in Actors)
                spr.Reset();

            mTextEffects.Clear();
            mCheckpointIndex = 1;
        }

        public void PlayerDeath()
        {
            LevelStatistics.Deaths++;
            Player.DeathReset();
            Portals.Reset();
            mTextEffects.Add(new DeathNotification(Vector2.Clamp(Player.Position, Vector2.Zero, mDimensions)));
            Player.Position = Checkpoints[mCheckpointIndex-1].Location;
        }

        public void PlaySoundEffect(SoundEffect sfx)
        {
            sfx.Play(mAudio.SoundEffectsVolume / 100f, 0f, 0f);
        }
        #endregion

        #region Private Methods
        private void GameOver()
        {
            PlaySoundEffect(mExitReached);
            Player.GameOver();
            throw new Exception("LevelComplete");
        }
        #endregion
        
        #region Static Methods
        public static Level LoadLevel(int levelIdentifier)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            try
            {
                FileStream fs = new FileStream(@"Content\Levels\Level" + levelIdentifier + ".xml", FileMode.Open);
                XmlReader reader = new XmlTextReader(fs);

                Level lvl = (Level)serializer.Deserialize(reader);

                fs.Close();
                reader.Close();

                return lvl;
            }
            catch
            {
                return null;       
            }
        }

        public static Level LoadLevel(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                XmlReader reader = new XmlTextReader(fs);

                Level lvl = (Level)serializer.Deserialize(reader);

                fs.Close();
                reader.Close();

                return lvl;
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}

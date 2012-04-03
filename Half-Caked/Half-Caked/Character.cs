using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    public class Character : Actor
    {
        public enum State
        {
            Immobile = -1,
            Ground = 0,
            Air = 1,
            Portal = 2,
            Platform = 3,
            GravityPortal = 4
        }

        #region Constants
        public static Guid CharacterGuid =
            new Guid("05159D8A-B739-4AA4-9F6D-3FF5CB29572D");

        const string ASSETNAME = "Sprites\\Player\\Idle";
        const int DEFAULT_SPEED = 250;
        const int DEFAULT_JUMP = 250;
        const int JUMP_HEIGHT_MAX = 350;
        const int MOVE_UP = -1;
        const int MOVE_DOWN = 1;
        const int MOVE_LEFT = -1;
        const int MOVE_RIGHT = 1;
        const float MASS = 40.0f;
        const float STATIC_ACCEL_GND = MASS * .20f;
        const float DYNAMIC_ACCEL_AIR = MASS * .20f;
        const float STATIC_ACCEL_AIR = MASS * .0025f;
        const float ARM_LENGTH = 20;
        #endregion

        #region Fields
        //Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation victoryAnimation;
        private Animation deathAnimation;
        private AnimationPlayer animator;

        State mCurrentState = State.Ground;

        PortalGunBullet[] mOrbs = new PortalGunBullet[2];
        Gunarm mGunhand;
        ContentManager mContentManager;

        bool mIsDucking = false, mForcedDucking;

        bool stillJumping = false; //currently performing normal jump
        bool wallJumpLeft = false; //walljump from right wall to left direction is available to player
        bool wallJumpRight = false; //walljump from left wall to right direction is available to player
        bool stillWallJumpingLeft = false; //currently walljumping left
        bool stillWallJumpingRight = false; //currently walljumping right
        TimeSpan jumpTimer = TimeSpan.Zero;
        TimeSpan wallJumpTimer = TimeSpan.Zero;

        float mCurrentFriction;
        Rectangle[] mCollisions = new Rectangle[5];

        SoundEffect mJumpEffect, mDeathEffect, mLandingEffect;
        #endregion

        #region Initialization
        public void LoadContent(ContentManager theContentManager)
        {
            mContentManager = theContentManager;
            base.LoadContent(theContentManager, ASSETNAME);

            mGunhand = new Gunarm();
            mGunhand.LoadContent(this.mContentManager);

            mOrbs[0] = new PortalGunBullet();
            mOrbs[0].LoadContent(this.mContentManager, 0);
            mOrbs[1] = new PortalGunBullet();
            mOrbs[1].LoadContent(this.mContentManager, 1);

            //Load animations -- must include frame count for constant frameTime constructor
			float[] jumpTiming = { 0.1f, 0.1f, 0.1f, 0.2f, 0.1f };
            idleAnimation = new Animation(theContentManager.Load<Texture2D>("Sprites\\Player\\Idle"), 0.1f, 1, true);
            runAnimation = new Animation(theContentManager.Load<Texture2D>("Sprites\\Player\\Run"), 0.05f, 5, true);
            jumpAnimation = new Animation(theContentManager.Load<Texture2D>("Sprites\\Player\\Jump"), jumpTiming, false);
            victoryAnimation = new Animation(theContentManager.Load<Texture2D>("Sprites\\Player\\Victory"), 0.1f, 11, false);
            deathAnimation = new Animation(theContentManager.Load<Texture2D>("Sprites\\Player\\Death"), 0.1f, 12, false);

            int width = (int)(idleAnimation.FrameWidth);// * 0.9f);
            int left = 0; //(idleAnimation.FrameWidth - width) / 2;
			int height = (int)(idleAnimation.FrameHeight);//* 0.8f);
            int top = idleAnimation.FrameHeight - height;
            //Source = new Rectangle(0, 0, 125, 125);
            Source = new Rectangle(left, top, width, height);

            animator.PlayAnimation(idleAnimation);

            mJumpEffect = theContentManager.Load<SoundEffect>("Sounds\\PlayerJump");
            mDeathEffect = theContentManager.Load<SoundEffect>("Sounds\\PlayerKilled");
            mLandingEffect = theContentManager.Load<SoundEffect>("Sounds\\PlayerLanding");

            Center = new Vector2(Size.Width / 2, Size.Height / 2);
        }
        #endregion

        #region Update and Draw

        public void Update(GameTime theGameTime, Level level, InputState inputState)
        {
            Acceleration = Vector2.Zero;

            var curstate = mCurrentState;
            CheckCollisions(level, inputState.IsInteracting(null));

            //play sound effect for landing
            if (curstate == State.Air && (mCurrentState == State.Ground || mCurrentState == State.Platform))
                level.PlaySoundEffect(mLandingEffect);

            UpdateMovement(inputState);

            //play sound effect for jumping
            if (UpdateJump(inputState, theGameTime))
                level.PlaySoundEffect(mJumpEffect);

            UpdateDuck(inputState);

            //falling
            Acceleration.Y = (mCurrentState == State.Air || mCurrentState == State.GravityPortal || mCurrentState == State.Portal ? 1 : 0) * level.Gravity * Level.METERS_TO_UNITS;

            if (Angle != 0)
            {
                if (Angle > 0)
                    Angle = (float)Math.Max(0, Angle - Math.PI * theGameTime.ElapsedGameTime.TotalSeconds);
                else
                    Angle = (float)Math.Min(0, Angle + Math.PI * theGameTime.ElapsedGameTime.TotalSeconds);

            }

            base.Update(theGameTime);

            mFlip = mGunhand.Update(inputState.CurrentMouseState, mIsDucking, this, level.Position);
            UpdateProjectile(theGameTime, inputState, level);
        }

        public void CheckCollisions(Level level, bool ePressed)
        {
            FrameVelocity = Vector2.Zero;

            if (level.Portals.IsOpen())
            {
                if (HandlePortalCollision(0, level) || HandlePortalCollision(1, level))
                    return;
            }

            if (mForcedDucking)
                StopDucking();
            mCurrentState = State.Air;

            foreach (Obstacle obs in level.Obstacles)
            {
                Rectangle result = Rectangle.Intersect(obs.CollisionSurface, CollisionSurface);
                if (!result.IsEmpty)
                {
                    if (HandleStandardCollision(result, obs.CollisionSurface, obs.Contact(result), obs.Friction * level.Gravity))
                    {
                        Die(level);
                        return;
                    }

                    var pltfrm = obs as Platform;
                    if (pltfrm != null && pltfrm.IsMoving)
                        FrameVelocity = obs.Velocity + Vector2.UnitY *  40;

                    if (ePressed)
                        obs.React(CharacterGuid, level);
                }
            }

            foreach (Tile tile in level.Tiles)
            {
                Rectangle result = Rectangle.Intersect(tile.Dimensions, CollisionSurface);
                if (!result.IsEmpty)
                {
                    if (HandleStandardCollision(result, tile.Dimensions, tile.Type, tile.Friction * level.Gravity))
                    {
                        Die(level);
                        return;
                    }
                }
            }

            if (mCollisions[(int)Orientation.Down] != Rectangle.Empty && mCollisions[(int)Orientation.Up] != Rectangle.Empty)
            {
                mForcedDucking = true;
                Duck();
                if (mCollisions[(int)Orientation.Down].Intersects(this.CollisionSurface) &&
                    mCollisions[(int)Orientation.Up].Intersects(this.CollisionSurface))
                    Die(level);
            }
            else
                mForcedDucking = false;

            for (int i = 0; i < 5; i++)
                mCollisions[i] = Rectangle.Empty;
        }

        public override void PortalUpdateDependents()
        {
            mGunhand.Update(mIsDucking, this);
        }

        public override void Draw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            if (!Visible)
                return;
            //base.Draw(theSpriteBatch, Relative);
            mOrbs[0].Draw(theSpriteBatch, Relative);
            mOrbs[1].Draw(theSpriteBatch, Relative);
            mGunhand.Draw(theSpriteBatch, Relative);
        }

        public void AnimatedDraw(SpriteBatch theSpriteBatch, Vector2 Relative, GameTime gameTime)
        {
            // Draw the sprite.
            animator.Draw(gameTime, theSpriteBatch, Position + Relative, Angle, Center, Scale, mFlip);
        }

        public override void PortalDraw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            base.PortalDraw(theSpriteBatch, Relative);
            mGunhand.PortalDraw(theSpriteBatch, Relative);
        }

        #endregion

        #region Public Methods
        public override void Reset()
        {
            base.Reset();
            mOrbs[0].Reset();
            mOrbs[1].Reset();
            mGunhand.Reset();

            mCurrentState = State.Ground;
            StopDucking();

            animator.PlayAnimation(idleAnimation);
        }

        public void Die(Level level)
        {
            //animator.PlayAnimation(deathAnimation);
            level.PlaySoundEffect(mDeathEffect);
            level.PlayerDeath();
        }

        public void DeathReset()
        {
            base.Reset();
            mForcedDucking = false;
            StopDucking();
        }

        public bool IsGrounded()
        {
            return State.Platform == mCurrentState || State.Ground == mCurrentState;
        }

        #endregion

        #region Private Methods
        private bool HandlePortalCollision(int portalNumber, Level level)
        {
            Sprite Portal = (portalNumber == 0 ? level.Portals.Portal1 : level.Portals.Portal2);

            Rectangle result = Rectangle.Intersect(CollisionSurface, Portal.CollisionSurface);
            if (!result.IsEmpty)
            {
                // Horizontal
                if ((int)Portal.Oriented % 2 == 1)
                {
                    if (result.Height >= this.CollisionSurface.Height - 3 ||
                        mCurrentState == State.Portal || mCurrentState == State.GravityPortal)
                    {
                        mCurrentState = State.Portal;
                        level.Portals.AddSprite(portalNumber, this);
                        FrameVelocity = Portal.FrameVelocity;

                        bool first;
                        if ((first = (CollisionSurface.Y >= Portal.CollisionSurface.Bottom - CollisionSurface.Height)) || Portal.CollisionSurface.Y >= CollisionSurface.Y)
                        {
                            if (first)
                                Velocity.Y = Math.Min(Velocity.Y, 0);
                            else
                            {
                                Velocity.Y = Math.Max(Velocity.Y, 0);
                                mCurrentState = State.GravityPortal;
                                stillJumping = false;
                            }

                            Position = new Vector2(Position.X, MathHelper.Clamp(Position.Y, Portal.CollisionSurface.Y + Center.Y, Portal.CollisionSurface.Bottom - Center.Y));
                        }
                        return true;
                    }
                }
                //Vertical
                else
                {
                    if (result.Width >= this.CollisionSurface.Width - 1 ||
                        mCurrentState == State.Portal || mCurrentState == State.GravityPortal)
                    {
                        mCurrentState = State.GravityPortal;
                        level.Portals.AddSprite(portalNumber, this);

                        if (CollisionSurface.X > Portal.CollisionSurface.Right - CollisionSurface.Width || Portal.CollisionSurface.X > CollisionSurface.X)
                        {
                            Position = new Vector2(MathHelper.Clamp(Position.X, Portal.CollisionSurface.X + Center.X, Portal.CollisionSurface.Right - Center.X), Position.Y);
                            Velocity.X = 0;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HandleStandardCollision(Rectangle result, Rectangle obj, Surface type, float friction)
        {
            if (3 >= result.Height || result.Width >= this.CollisionSurface.Width - 1
                || result.Height / (float)CollisionSurface.Height < result.Width / (float)CollisionSurface.Width)
            {
                if (CollisionSurface.Center.Y < obj.Center.Y)
                {
                    mCurrentFriction = friction * MASS;
                    mCurrentState = State.Ground;
                    Velocity.Y = 0;
                    Position = new Vector2(Position.X, obj.Top - Center.Y + 1);
                    mCollisions[(int)Orientation.Down] = obj;
                }
                else if (CollisionSurface.Center.Y > obj.Center.Y) // hit the roof
                {
                    if (Velocity.Y < 5)
                    { Velocity.Y = 5; }
                }
                else
                {
                    Velocity.Y = Math.Min(Velocity.Y, 0);
                    Position = new Vector2(Position.X, obj.Bottom + Center.Y + 1);
                    mCollisions[(int)Orientation.Up] = obj;
                }
                stillJumping = false;
                stillWallJumpingLeft = false;
                stillWallJumpingRight = false;
            }
            else
            {
                if (Position.X > result.X)
                {
                    mCollisions[(int)Orientation.Left] = obj;
                    wallJumpRight = true; //walljump from left wall to right direction is available to player
                    if (Velocity.X < 0)
                        Velocity.X = 0;
                }
                else if (Position.X < result.X)
                {
                    mCollisions[(int)Orientation.Right] = obj;
                    wallJumpLeft = true; //walljump from right wall to left direction is available to player
                    if (Velocity.X > 0)
                        Velocity.X = 0;
                }

                Position = new Vector2(result.X - (Position.X - result.X < 0 ? CollisionSurface.Width - Center.X - 1 : -result.Width + 1 - Center.X), Position.Y);
            }

            return type == Surface.Death;
        }

        //UNDER CONSTRUCTION                                   
        private void UpdateMovement(InputState inputState)
        {
            //Movement while on the ground is UNDER CONSTRUCTION (values are hardcoded) 
            if (mCurrentState == State.Ground || mCurrentState == State.Platform || mCurrentState == State.Portal)
            {
                if (Math.Abs(Velocity.X) <= STATIC_ACCEL_GND)
                {
                    Acceleration.X = 0;
                    Velocity.X = 0;
                }
                else
                    Acceleration.X = mCurrentFriction * (-Math.Sign(Velocity.X));

                if (inputState.IsMovingBackwards(null))
                {
                    Velocity.X = DEFAULT_SPEED * MOVE_LEFT * (mIsDucking ? .5f : 1);
                    if (mIsDucking)
                    { }
                    else
                    { animator.PlayAnimation(runAnimation); }
                }
                else if (inputState.IsMovingForward(null))
                {
                    Velocity.X = DEFAULT_SPEED * MOVE_RIGHT * (mIsDucking ? .5f : 1);
                    if (mIsDucking)
                    { }
                    else
                    { animator.PlayAnimation(runAnimation); }
                }
                else
                {
                    animator.PlayAnimation(idleAnimation);
                }
            }


            //Movement while in the AIR is UNDER CONSTRUCTION (values are hardcoded)
            else if (mCurrentState == State.Air || mCurrentState == State.GravityPortal)
            {
                if (Math.Abs(Velocity.X) <= STATIC_ACCEL_AIR)
                {
                    Acceleration.X = 0;
                    Velocity.X = 0;
                }
                else
                {
                    Acceleration.X = DYNAMIC_ACCEL_AIR * (Math.Abs(Velocity.X) <= STATIC_ACCEL_AIR ? 0 : 1) * (-Math.Sign(Velocity.X));
                }

                if (inputState.IsMovingBackwards(null))
                {
                    //Acceleration.X += DYNAMIC_ACCEL_AIR * MOVE_LEFT / 4;
                    //Velocity.X = Math.Min(Velocity.X, DEFAULT_SPEED / 2f * MOVE_LEFT * (mIsDucking ? .5f : 1));
                    if (Velocity.X >= -DEFAULT_SPEED)
                    {
                        if (Velocity.X > 0)
                        {
                            Velocity.X -= 50;
                        }
                        else
                        {
                            Velocity.X -= 25;
                        }
                    }

                    animator.PlayAnimation(jumpAnimation);
                }
                else if (inputState.IsMovingForward(null))
                {
                    //Acceleration.X += DYNAMIC_ACCEL_AIR * MOVE_RIGHT / 4;
                    //Velocity.X = Math.Max(Velocity.X, DEFAULT_SPEED / 2f * MOVE_RIGHT * (mIsDucking ? .5f : 1));
                    if (Velocity.X <= DEFAULT_SPEED)
                    {
                        if (Velocity.X < 0)
                        {
                            Velocity.X += 50;
                        }
                        else
                        {
                            Velocity.X += 25;
                        }
                    }

                    animator.PlayAnimation(jumpAnimation);
                }

                //if (mIsDucking)
                //    Acceleration.X *= .75f;
                //}
            }
        }

        //UNDER CONSTRUCTION
        private bool UpdateJump(InputState inputState, GameTime theGameTime)
        {
            //start jump
            if ((mCurrentState == State.Ground || mCurrentState == State.Platform) && Velocity.Y == 0)
            {
                if (inputState.IsNewJump(null))
                {
                    mCurrentState = State.Air;
                    Velocity.Y = -DEFAULT_JUMP;
                    stillJumping = true;
                    jumpTimer = TimeSpan.Zero;
                    animator.PlayAnimation(jumpAnimation);
                    return true;
                }
            }

            //walljumping
            if (mCurrentState == State.Air)
            {
                if (wallJumpRight) //walljump from left wall to right direction is available to player
                {
                    if (inputState.IsNewJump(null))
                    {
                        Velocity.X = DEFAULT_SPEED * MOVE_RIGHT * 2f;
                        //Velocity.Y = -DEFAULT_JUMP / 2;
                        Velocity.Y = -DEFAULT_JUMP;

                        stillJumping = true;
                        stillWallJumpingRight = true;
                        jumpTimer = TimeSpan.Zero;
                        wallJumpTimer = TimeSpan.Zero;
                        animator.ResetAnimation();
                        return true;
                    }
                }
                else if (wallJumpLeft) //walljump from right wall to left direction is available to player
                {
                    if (inputState.IsNewJump(null))
                    {
                        Velocity.X = DEFAULT_SPEED * MOVE_LEFT * 2f;
                        //Velocity.Y = -DEFAULT_JUMP / 2;
                        Velocity.Y = -DEFAULT_JUMP;

                        stillJumping = true;
                        stillWallJumpingLeft = true;
                        jumpTimer = TimeSpan.Zero;
                        wallJumpTimer = TimeSpan.Zero;
                        animator.ResetAnimation();
                        return true;
                    }
                }

                if (stillWallJumpingRight)
                {
                    wallJumpTimer += theGameTime.ElapsedGameTime;
                    if (wallJumpTimer < TimeSpan.FromMilliseconds(JUMP_HEIGHT_MAX / 2))
                    {
                        Velocity.X = DEFAULT_SPEED * MOVE_RIGHT * 2f;
                        //Velocity.Y = -DEFAULT_JUMP/2;
                        Velocity.Y = -DEFAULT_JUMP;
                    }
                    else
                    {
                        stillWallJumpingRight = false;
                        stillJumping = false;
                    }
                }
                else if (stillWallJumpingLeft)
                {
                    wallJumpTimer += theGameTime.ElapsedGameTime;
                    if (wallJumpTimer < TimeSpan.FromMilliseconds(JUMP_HEIGHT_MAX / 2))
                    {
                        Velocity.X = DEFAULT_SPEED * MOVE_LEFT * 2f;
                        //Velocity.Y = -DEFAULT_JUMP/2;
                        Velocity.Y = -DEFAULT_JUMP;
                    }
                    else
                    {
                        stillWallJumpingLeft = false;
                        stillJumping = false;
                    }
                }
            }
            else
            {
                stillWallJumpingRight = false;
                stillWallJumpingLeft = false;
                stillJumping = false;
            }

            //variable height jump
            if (stillJumping && !stillWallJumpingRight && !stillWallJumpingLeft)
            {
                if (inputState.IsJumping(null))
                {
                    mCurrentState = State.Air;

                    //Velocity.Y = -DEFAULT_JUMP;
                    Velocity.Y = -DEFAULT_JUMP;

                    jumpTimer += theGameTime.ElapsedGameTime;

                    if (jumpTimer > TimeSpan.FromMilliseconds(JUMP_HEIGHT_MAX))
                    {
                        stillJumping = false;

                        jumpTimer = TimeSpan.Zero;
                    }
                }

                else //player let go of jump key early for a shorter jump
                {
                    stillJumping = false;
                }
            }

            wallJumpLeft = false;
            wallJumpRight = false;

            return false;
        }

        private void UpdateDuck(InputState inputState)
        {
            if (inputState.IsDucking(null) || mForcedDucking)
            {
                Duck();
            }
            else
            {
                StopDucking();
            }
        }

        private void Duck()
        {
            if (!mIsDucking)
            {
                //Position += new Vector2(0 ,16);
                //Source = new Rectangle(272, 97, 67, 103);
                //Center = new Vector2(Size.Width / 2, Size.Height / 2);
                //mIsDucking = true;
            }
        }

        private void StopDucking()
        {
            if (mIsDucking)
            {
                //Position -= new Vector2(0, 16);
                //Source = new Rectangle(66, 65, 72, 135);
                //Center = new Vector2(Size.Width / 2, Size.Height / 2);
                //mIsDucking = false;
            }
        }

        private void UpdateProjectile(GameTime theGameTime, InputState inputState, Level level)
        {
            if (mOrbs[0].Visible)
            {
                mOrbs[0].Update(theGameTime);
                mOrbs[0].CheckCollisions(level);
            }
            else if (inputState.IsFiringPortal1(null) && level.Portals.CanClose())
            {
                ShootProjectile(0, inputState.CurrentMouseState, level);
                mOrbs[0].CheckCollisions(level);
            }

            if (mOrbs[1].Visible)
            {
                mOrbs[1].Update(theGameTime);
                mOrbs[1].CheckCollisions(level);
            }
            else if (inputState.IsFiringPortal2(null) && level.Portals.CanClose())
            {
                ShootProjectile(1, inputState.CurrentMouseState, level);
                mOrbs[1].CheckCollisions(level);
            }
        }

        private void ShootProjectile(int type, MouseState aCurrentMouseState, Level level)
        {
            Vector2 dir = new Vector2((float)Math.Cos(mGunhand.Angle), (float)Math.Sin(mGunhand.Angle));

            mOrbs[type].Fire(mGunhand.Position + ARM_LENGTH * dir,
                            dir,
                            Vector2.Zero,
                            level);
        }
        #endregion
    }

    class Gunarm : Actor
    {
        #region Constants
        Vector2 ARM_ANCHOR = new Vector2(-10, 10);
        Vector2 ARM_ANCHOR_OFFSET = new Vector2(4, 4);
        Vector2 ARM_ANCHOR_DUCKED = new Vector2(0,0);

        Vector2 ARM_ANCHOR_LEFT = new Vector2(25, 0);
        Vector2 ARM_ANCHOR_LEFT_OFFSET = new Vector2(4, 15);
        Vector2 ARM_ANCHOR_DUCKED_LEFT = new Vector2(0, 0);
        #endregion

        #region Fields

        bool wasFlipped = false;

        #endregion

        #region Initialization

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, "Sprites\\Gunarm");
            Center = new Vector2(4, 4);
        }

        #endregion

        #region Update and Draw

        public SpriteEffects Update(MouseState aCurrentMouseState, bool ducking, Character theMan, Vector2 rel)
        {
            if(wasFlipped)
                Position = theMan.Position + Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED + ARM_ANCHOR_DUCKED_LEFT : ARM_ANCHOR + ARM_ANCHOR_LEFT), Matrix.CreateRotationZ(theMan.Angle)) ;
            else
                Position = theMan.Position + Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED : ARM_ANCHOR), Matrix.CreateRotationZ(theMan.Angle));

            bool flip = aCurrentMouseState.X < Position.X + rel.X;
            if (flip && !wasFlipped)
                Position += Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED_LEFT : ARM_ANCHOR_LEFT), Matrix.CreateRotationZ(theMan.Angle));
            else if (!flip && wasFlipped)
                Position -= Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED_LEFT : ARM_ANCHOR_LEFT), Matrix.CreateRotationZ(theMan.Angle));

            wasFlipped = flip;

            Vector2 displacement =  (Position + rel + (flip ? ARM_ANCHOR_OFFSET : ARM_ANCHOR_LEFT_OFFSET));
            Angle = (float)Math.Atan2(aCurrentMouseState.Y - displacement.Y, aCurrentMouseState.X - displacement.X);

            if (flip)
            {
                Center = new Vector2(4, 15);
                mFlip = SpriteEffects.FlipVertically;
                return SpriteEffects.None;
            }
            else
            {
                Center = new Vector2(4, 4);
                mFlip = SpriteEffects.None;
                return SpriteEffects.FlipHorizontally;
            }
        }

        public void Update(bool ducking, Character theMan)
        {
            PortalPosition = theMan.PortalPosition + Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED : ARM_ANCHOR), Matrix.CreateRotationZ(theMan.PortalAngle));

            if (Math.Abs(MathHelper.WrapAngle(Angle)) > MathHelper.PiOver2)
                PortalPosition += Vector2.Transform((ducking ? ARM_ANCHOR_DUCKED_LEFT : ARM_ANCHOR_LEFT), Matrix.CreateRotationZ(theMan.PortalAngle));

            PortalAngle = Angle + theMan.PortalAngle;
        }

        #endregion
    }
}
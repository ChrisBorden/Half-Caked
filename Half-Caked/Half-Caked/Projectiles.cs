using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Half_Caked
{
    abstract class Projectiles : Sprite
    {
        #region Fields
        protected float mSpeed = 1000f;
        protected SoundEffect mFireSound;
        #endregion
        
        #region Public Methods
        public void Fire(Vector2 startPosition, Vector2 direction, Vector2 acceleration, Level lvl)
        {
            if(mFireSound != null)
                lvl.PlaySoundEffect(mFireSound);
            Position = startPosition;
            Velocity = direction * mSpeed;
            Acceleration = acceleration;
            Visible = true;
        }

        public void CheckCollisions(Level level)
        {
            if (!Visible)
                return;

            if (Position.X != MathHelper.Clamp(Position.X, 0, level.Size.Width - Size.Width) ||
                Position.Y != MathHelper.Clamp(Position.Y, 0, level.Size.Height - Size.Height))
            {
                Visible = false;
                return;
            }

            if (CollisionSurface.Intersects(level.Player.CollisionSurface))
            {
                HandlePlayerCollision(level);
                if (!Visible)
                    return;
            }

            foreach (Tile tile in level.Tiles)
            {
                Rectangle result = Rectangle.Intersect(tile.Dimensions, CollisionSurface);
                if (!result.IsEmpty)
                {
                    HandleTileCollision(tile, result, level);
                    if (!Visible)
                        return;
                }
            }

            foreach (Obstacle obs in level.Obstacles)
            {
                Rectangle result = Rectangle.Intersect(obs.CollisionSurface, CollisionSurface);
                if (!result.IsEmpty)
                {
                    HandleObstacleCollision(obs, result, level);
                    if (!Visible)
                        return;
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            Visible = false;
        }
        #endregion

        #region Private Methods
        abstract protected void HandleTileCollision(Tile tile, Rectangle result, Level level);
        abstract protected void HandleObstacleCollision(Obstacle obs, Rectangle result, Level level);
        abstract protected void HandlePlayerCollision(Level level);
        #endregion
    }

    class PortalGunBullet : Projectiles
    {
        #region Fields
        private int mPortalNumber;
        #endregion

        #region Initialization
        public void LoadContent(ContentManager theContentManager, int classification)
        {
            base.LoadContent(theContentManager, "Sprites\\PortalBullets");
            mFireSound = theContentManager.Load<SoundEffect>("Sounds\\PortalFire");
            Source = new Rectangle(200 * classification, 0, 200, Source.Height);
            Center = new Vector2(10, 10);
            Scale = 0.1f;
            Visible = false;
            mPortalNumber = classification;
        }
        #endregion

        #region Public Methods

        public override void Reset()
        {
            base.Reset();
            Scale = .1f;
        }

        #endregion

        #region Private Methods
        protected override void HandlePlayerCollision(Level level) { /*Do nothing*/ }

        protected override void HandleTileCollision(Tile tile, Rectangle result, Level level)
        {
            HandleCollision(tile.Dimensions, result, level, tile.Type, Vector2.Zero, tile);
        }

        protected override void HandleObstacleCollision(Obstacle obs, Rectangle result, Level level)
        {
            HandleCollision(obs.CollisionSurface, result, level, obs.Contact(result), obs.Velocity, obs);
        }

        private void HandleCollision(Rectangle collisionSurface, Rectangle result, Level level, Surface type, Vector2 frameVelocity, object targetObj)
        {
            switch (type)
            {
                case Surface.Amplifies:
                    level.Portals.Amplify(mPortalNumber, true);
                    Act(collisionSurface, result, level, frameVelocity, targetObj);
                    break;
                case Surface.Normal:
                    level.Portals.Amplify(mPortalNumber, false);
                    Act(collisionSurface, result, level, frameVelocity, targetObj);
                    break;
                case Surface.Reflects:
                    Reflect(result);
                    break;
                default:
                    Absorb();
                    break;
            }
        }

        protected void Absorb()
        {
            Velocity = Vector2.Zero;
            Visible = false;
        }

        protected void Act(Rectangle target, Rectangle result, Level level, Vector2 targetVelocity, object targetObj)
        {
            Visible = false;
            Orientation orientation;
            Vector2 openingPoint;

            bool validXCollision = (Velocity.X > targetVelocity.X == result.Center.X < target.Center.X) || (target.Width - Math.Abs(Velocity.X) / 30 <= 0);
            bool validYCollision = (Velocity.Y > targetVelocity.Y == result.Center.Y < target.Center.Y) || (target.Height - Math.Abs(Velocity.Y) / 30 <= 0);

            if (result.Width < result.Height && validXCollision)
            {
                if (PortalGroup.PORTAL_HEIGHT > target.Height)
                {
                    Absorb();
                    return;
                }
                if (Velocity.X > targetVelocity.X)
                {
                    orientation = Orientation.Left;
                    openingPoint.X = target.Left - 1;
                }
                else
                {
                    orientation = Orientation.Right;
                    openingPoint.X = target.Right - PortalGroup.PORTAL_WIDTH + 1;
                }

                openingPoint.Y = MathHelper.Clamp(result.Center.Y, target.Top + PortalGroup.PORTAL_HEIGHT / 2, target.Bottom - PortalGroup.PORTAL_HEIGHT / 2);
            }
            else if (validYCollision)
            {
                if (PortalGroup.PORTAL_HEIGHT > target.Width)
                {
                    Absorb();
                    return;
                }
                if (Velocity.Y > targetVelocity.Y)
                {
                    orientation = Orientation.Up;
                    openingPoint.Y = target.Top - 1;
                }
                else
                {
                    orientation = Orientation.Down;
                    openingPoint.Y = target.Bottom - PortalGroup.PORTAL_WIDTH + 1;
                }

                openingPoint.X = MathHelper.Clamp(result.Center.X, target.Left + PortalGroup.PORTAL_HEIGHT / 2, target.Right - PortalGroup.PORTAL_HEIGHT / 2);
            }
            else
            {
                Visible = true;
                return;
            }
            Velocity = Vector2.Zero;
            level.Portals.Open(openingPoint, orientation, mPortalNumber, FrameVelocity, level, targetObj);
        }

        protected void Reflect(Rectangle result)
        {
            if (result.Width < result.Height)
            {
                Position = new Vector2(result.X - (Position.X < result.X ? Size.Width : -result.Width-2), Position.Y);
                Velocity *= new Vector2(-1, 1);
            }
            else
            {
                Position = new Vector2(Position.X, result.Y - (Position.Y < result.Y ? Size.Height : -result.Height-2));
                Velocity *= new Vector2(1, -1);
            }
        }
        #endregion
    }
    
    class EnemyBullet : Projectiles
    {
        #region Initialization
        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, "Sprites\\EnemyBullet");
            mFireSound = theContentManager.Load<SoundEffect>("Sounds\\EnemyFire");
            Source = new Rectangle(0, 0, 20, 9);
            Center = new Vector2(10, 4.5f);
            mSpeed = 200;
            Visible = false;
        }
        #endregion

        protected override void HandleTileCollision(Tile tile, Rectangle result, Level level)
        {
            this.Visible = false;
            Velocity = Acceleration = Vector2.Zero;
        }

        protected override void HandleObstacleCollision(Obstacle obs, Rectangle result, Level level)
        {
            this.Visible = false;
            Velocity = Acceleration = Vector2.Zero;
        }

        protected override void HandlePlayerCollision(Level level)
        {
            this.Visible = false;
            Velocity = Acceleration = Vector2.Zero;
            level.Player.Die(level);
        }
    }
}
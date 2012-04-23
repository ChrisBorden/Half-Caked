using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Half_Caked
{
    public class PortalGroup
    {
        private enum PortalState
        {
            Closed,
            Open,
            InUse
        }

        #region Constants
        public static float PORTAL_HEIGHT = 150;
        public static float PORTAL_WIDTH = 5;
        #endregion

        #region Fields
        public Sprite Portal1
        {
            get { return mPortals[0]; }
        }

        public Sprite Portal2
        {
            get { return mPortals[1]; }
        }

        public object Portal1Holder
        {
            get { return mPortalHolders[0]; }
        }

        public object Portal2Holder
        {
            get { return mPortalHolders[1]; }
        }              

        Sprite PortalEffect;
        PortalState mState = PortalState.Closed;
        List<Actor> mInPortal1 = new List<Actor>();
        List<Actor> mInPortal2 = new List<Actor>();

        object[] mPortalHolders = new object[2];
        Sprite[] mPortals = new Sprite[2];
        bool[] mIsAmplified = new bool[2];
        SoundEffect mOpenPortalEffect;
        #endregion

        #region Initialization
        public PortalGroup()
        {
            mPortals[0] = new Sprite();
            mPortals[1] = new Sprite();
            PortalEffect = new Sprite();

            Portal1.Center  = new Vector2(0, PORTAL_HEIGHT / 2);
            Portal2.Center = new Vector2(0, PORTAL_HEIGHT / 2);

            Portal1.Visible = Portal2.Visible = false;
        }

        public void LoadContent(ContentManager theContentManager)
        {
            Portal1.LoadContent(theContentManager, "Sprites\\Portal1");
            Portal2.LoadContent(theContentManager, "Sprites\\Portal2");
            PortalEffect.LoadContent(theContentManager, "Sprites\\PortalEffect");
            mOpenPortalEffect  = theContentManager.Load<SoundEffect>("Sounds\\PortalOpen");

            PortalEffect.Center = new Vector2(0, PortalEffect.Size.Height / 2);
        }
        #endregion

        #region Public Methods
        public bool IsOpen()
        {
            return mState != PortalState.Closed;
        }

        public bool CanClose()
        {
            return mState != PortalState.InUse;
        }

        public void Amplify(int portalNumber, bool isAmplified)
        {
            mIsAmplified[portalNumber] = isAmplified;
        }

        public void Amplify(int portalNumber)
        {
            Amplify(portalNumber, true);
        }

        public void Open(Vector2 position, Orientation orientation, int portalNumber, Vector2 movement, Level lvl, object targetObject)
        {
            Sprite chosenOne = mPortals[portalNumber];
            
            try
            {
                mPortalHolders[portalNumber] = targetObject;
                chosenOne.Visible = true;
                chosenOne.Angle = (int)orientation % 2== 0 ? MathHelper.PiOver2 : 0;
                chosenOne.Position = position;
                chosenOne.Oriented = orientation;
                chosenOne.FrameVelocity = movement;

                int count = 0;

                foreach (Tile t in lvl.Tiles)
                    if (!Rectangle.Intersect(chosenOne.CollisionSurface, t.Dimensions).IsEmpty)
                        ++count;

                if (count > 1)
                    throw new Exception("Invalid Portal Location");
            }
            catch
            {
                    return;
            }

            if (Portal1.Visible == Portal2.Visible)
            {
                if (!Rectangle.Intersect(Portal1.CollisionSurface, Portal2.CollisionSurface).IsEmpty)
                {
                    Close(1 - portalNumber);
                    return;
                }
                mState = PortalState.Open;
                lvl.PlaySoundEffect(mOpenPortalEffect);
            }
            lvl.LevelStatistics.PortalsOpened++;
        }

        public void Close(int portalNumber)
        {
            mPortalHolders[portalNumber] = null;
            Sprite chosenOne = mPortals[portalNumber];
            chosenOne.Angle = 0;
            chosenOne.Position = new Vector2(-50, -50);
            chosenOne.Visible = false;
            chosenOne.Scale = 1f;
            mState = PortalState.Closed;
        }

        public void Reset()
        {
            Portal1.Reset();
            Portal2.Reset();
            mState = PortalState.Closed;
            mInPortal1.Clear();
            mInPortal2.Clear();
            Portal1.Visible = Portal2.Visible = false;
        }

        public void AddSprite(int portalNumber, Actor spr)
        {
            (portalNumber == 0 ? mInPortal1 : mInPortal2).Add(spr);
            mState = PortalState.InUse;
        }

        public void ClearSprites()
        {
            mInPortal1.Clear();
            mInPortal2.Clear();
        }
        #endregion

        #region Draw and Update
        public void Update(GameTime theGameTime)
        {            
            if (mInPortal1.Count + mInPortal2.Count > 0)
                mState = PortalState.InUse;
            else if(mState == PortalState.InUse)
                mState = PortalState.Open;

            Portal1.Update(theGameTime);
            Portal2.Update(theGameTime);

            foreach (Actor spr in mInPortal1)
            {
                HandleSpriteInPortal(spr, Portal1, Portal2, mIsAmplified[0], (float)theGameTime.TotalGameTime.TotalSeconds);
            }

            foreach (Actor spr in mInPortal2)
            {
                HandleSpriteInPortal(spr, Portal2, Portal1, mIsAmplified[1], (float)theGameTime.TotalGameTime.TotalSeconds);
            }
        }

        public void Draw(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            if (IsOpen())
            {
                PortalEffect.Position = Portal1.Position;
                PortalEffect.Angle = Portal1.Angle + ((int)Portal1.Oriented == 3 ? MathHelper.Pi : (int)Portal1.Oriented == 0 ? MathHelper.Pi : 0);
                PortalEffect.Draw(theSpriteBatch, Relative);

                PortalEffect.Position = Portal2.Position;
                PortalEffect.Angle = Portal2.Angle + ((int)Portal2.Oriented == 3 ? MathHelper.Pi : (int)Portal2.Oriented == 0 ? MathHelper.Pi : 0);
                PortalEffect.Draw(theSpriteBatch, Relative);
            }

            foreach (Actor spr in mInPortal1)
            {
                spr.PortalDraw(theSpriteBatch, Relative);
            }

            foreach (Actor spr in mInPortal2)
            {
                spr.PortalDraw(theSpriteBatch, Relative);
            }
        }

        public void DrawPortals(SpriteBatch theSpriteBatch, Vector2 Relative)
        {
            Portal1.Draw(theSpriteBatch, Relative);
            Portal2.Draw(theSpriteBatch, Relative);
        }
        #endregion

        #region Private Methods
        private void HandleSpriteInPortal(Actor spr, Sprite portalIn,  Sprite portalOut, bool amplify, float currentTime)
        {
            spr.PortalAngle = MathHelper.WrapAngle(MathHelper.PiOver2 * (2 - (portalIn.Oriented - portalOut.Oriented)));
            Matrix rotation = Matrix.CreateRotationZ(spr.PortalAngle);

            Vector2 dXY = new Vector2(spr.Position.X - portalIn.CollisionSurface.Center.X,
                                      spr.Position.Y - portalIn.CollisionSurface.Center.Y);


            spr.PortalPosition = new Vector2(portalOut.CollisionSurface.Center.X, portalOut.CollisionSurface.Center.Y) + Vector2.Transform(dXY, rotation);
            
            bool swap = false;
            switch (portalIn.Oriented)
            {
                case Orientation.Up:
                    swap = portalIn.CollisionSurface.Center.Y < spr.CollisionSurface.Center.Y;
                    break;
                case Orientation.Right:
                    swap = portalIn.CollisionSurface.Center.X > spr.CollisionSurface.Center.X;
                    break;
                case Orientation.Down:
                    swap = portalIn.CollisionSurface.Center.Y > spr.CollisionSurface.Center.Y;
                    break;
                case Orientation.Left:
                    swap = portalIn.CollisionSurface.Center.X < spr.CollisionSurface.Center.X;
                    break;
                default:
                    break;
            }

            if (swap && (currentTime - spr.LastSwapTime > .2))
            {
                spr.LastSwapTime = currentTime;

                var temp = spr.PortalPosition;
                spr.PortalPosition = spr.Position;
                spr.Position = temp;

                var temp2 = spr.PortalAngle;
                spr.PortalAngle = spr.Angle;
                spr.Angle = temp2;

                spr.Velocity = Vector2.Transform(spr.Velocity, rotation) * (amplify ? 2 : 1);
                spr.Acceleration = Vector2.Transform(spr.Acceleration, rotation);

                if (portalOut.Oriented == Orientation.Up)
                    spr.Velocity += 25 * new Vector2(0, -1);
            }

            spr.PortalUpdateDependents();
        }
        #endregion
    }
}

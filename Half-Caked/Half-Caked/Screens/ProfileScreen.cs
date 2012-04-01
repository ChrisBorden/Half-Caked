using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;

namespace Half_Caked
{
    class ProfileScreen : MenuScreen
    {
        private List<Profile> mProfiles;
        private StorageDevice mDevice;
        private MenuEntry indicator;
        private int mDefault = -1;
        private int mNextProfile = 0;

        public event EventHandler<PlayerIndexEventArgs> ProfileSelected;

        public int Default
        {
            get { return mDefault; }
            set
            {
                mDefault = value;
                int index = mProfiles.FindIndex(x => x.ProfileNumber == mDefault);
                if (index >= 0)
                {
                    indicator.Text = "*";
                    indicator.Position = new Vector2(indicator.Position.X, MenuEntries[index].Position.Y - 12);
                }
                else
                {
                    indicator.Text = "";
                }
            }
        }

        public ProfileScreen(StorageDevice device)
            : base("Profile Management")
        {
            mDevice = device;
            indicator = new MenuEntry("");
            indicator.Position = new Vector2(70, -100);
            indicator.State = UIState.Selected;

            var temp = Profile.LoadAll(mDevice);

            mProfiles = temp.Value;

            string[] profileOptions = { "Delete", "Rename", "Make Active" };
            for (int i = 0; i < mProfiles.Count; i++)
            {
                if (mProfiles[i].ProfileNumber >= mNextProfile)
                    mNextProfile = mProfiles[i].ProfileNumber + 1;

                ButtonGroup profileGroup = new ButtonGroup(mProfiles[i].Name, profileOptions);
                profileGroup.HideInactive = true;

                profileGroup.Buttons[0].Pressed += ConfirmDeleteProfile(mProfiles[i]);
                profileGroup.Buttons[1].Pressed += RenameProfile(mProfiles[i]);
                profileGroup.Buttons[2].Pressed += SetDefaultProfile(mProfiles[i]);

                MenuEntries.Add(profileGroup);
            }

            MenuEntry addMenuEntry = new MenuEntry("Add Profile");
            addMenuEntry.Pressed += MakeProfile;
            MenuEntries.Add(addMenuEntry);

            MenuEntry backMenuEntry = new MenuEntry("Back");
            backMenuEntry.Pressed += OnCancel;
            MenuEntries.Add(backMenuEntry);

            Default = temp.Key;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            Default = mDefault;
        }

        EventHandler<PlayerIndexEventArgs> RenameProfile(Profile prof)
        {
            return (object sender, PlayerIndexEventArgs e) =>
            {
                InputDialog dialog = new InputDialog("Please rename the profile:", prof.Name);
                dialog.Buttons[0].Pressed += DoneRenaming(dialog, prof);
                ScreenManager.AddScreen(dialog, ControllingPlayer);
            };
        }

        EventHandler<PlayerIndexEventArgs> DoneRenaming(InputDialog dialog, Profile prof)
        {
            return (object sender, PlayerIndexEventArgs e) =>
            {
                prof.Name = dialog.Content;
                int index = mProfiles.IndexOf(prof);
                (MenuEntries[index] as ButtonGroup).Label.Text = prof.Name;

                if (mDefault == prof.ProfileNumber)
                    Profile.SaveProfile(prof, "default.sav", mDevice);
                else
                    Profile.SaveProfile(prof, "profile" + prof.ProfileNumber + ".sav", mDevice);
                PositionElements();
            };
        }

        EventHandler<PlayerIndexEventArgs> ConfirmDeleteProfile(Profile prof)
        {
            return (object sender, PlayerIndexEventArgs e) =>
            {
                ContentBoxScreen confirmDeleteMessageBox = new ContentBoxScreen("Are you sure you want to delete Profile:", prof.Name);

                confirmDeleteMessageBox.Buttons[0].Pressed += DeleteProfile(prof);
                ScreenManager.AddScreen(confirmDeleteMessageBox, e.PlayerIndex);
            };
        }

        EventHandler<PlayerIndexEventArgs> DeleteProfile(Profile prof)
        {
            return (object sender, PlayerIndexEventArgs e) =>
            {
                int index = mProfiles.IndexOf(prof);
                prof.Delete(mDevice);
                if (prof.ProfileNumber == mDefault)
                {
                    Default = -1;
                    (this.ScreenManager.Game as HalfCakedGame).CurrentProfile = new Profile();
                }

                mProfiles.RemoveAt(index);
                MenuEntries.RemoveAt(index);
                PositionElements();
            };
        }

        EventHandler<PlayerIndexEventArgs> SetDefaultProfile(Profile prof)
        {
            return (object sender, PlayerIndexEventArgs e) =>
            {
                Profile.ChangeDefault(mDefault, prof.ProfileNumber, mDevice);
                Default = prof.ProfileNumber;

                if(ProfileSelected != null)
                    ProfileSelected.Invoke(this, e);

                (this.ScreenManager.Game as HalfCakedGame).CurrentProfile = prof;
            };
        }

        void MakeProfile(object sender, PlayerIndexEventArgs e)
        {
            InputDialog dialog = new InputDialog("Please Enter a name for your profile:", "");
            dialog.Buttons[0].Pressed += AddProfile(dialog);
            ScreenManager.AddScreen(dialog, ControllingPlayer);
        }

        EventHandler<PlayerIndexEventArgs> AddProfile(InputDialog dialog)
        {
            return (object sender, PlayerIndexEventArgs e) =>
            {
                Profile prof = new Profile();
                prof.Name = dialog.Content;
                prof.ProfileNumber = mNextProfile++;

                string[] profileOptions = { "Delete", "Rename", "Make Active" };
                ButtonGroup profileGroup = new ButtonGroup(prof.Name, profileOptions);
                profileGroup.HideInactive = true;
                profileGroup.LoadContent(this);

                profileGroup.Buttons[0].Pressed += ConfirmDeleteProfile(prof);
                profileGroup.Buttons[1].Pressed += RenameProfile(prof);
                profileGroup.Buttons[2].Pressed += SetDefaultProfile(prof);

                MenuEntries.Insert(MenuEntries.Count - 2, profileGroup);
                mProfiles.Add(prof);
                PositionElements();
                prof.Register();

                if (mProfiles.Count == 1)
                {
                    Default = prof.ProfileNumber;
                    Profile.SaveProfile(prof, "default.sav", mDevice);
                    (this.ScreenManager.Game as HalfCakedGame).CurrentProfile = prof;

                    if (ProfileSelected != null)
                        ProfileSelected.Invoke(this, e);
                }
                else
                    Profile.SaveProfile(prof, "profile" + prof.ProfileNumber + ".sav", mDevice);
            };
        }

        public override void Update(GameTime gameTime, bool topScreen, bool coveredByOtherScreen)
        {
            base.Update(gameTime, topScreen, coveredByOtherScreen);
            indicator.Update(this, true, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                indicator.Position = new Vector2(80 - transitionOffset * 256, indicator.Position.Y);
            else
                indicator.Position = new Vector2(80 + transitionOffset * 512, indicator.Position.Y);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();
            indicator.Draw(this, gameTime);
            spriteBatch.End();
        }
    }
}

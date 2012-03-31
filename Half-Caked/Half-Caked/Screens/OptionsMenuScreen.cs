#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
<<<<<<< HEAD
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
=======
using System.Collections.Generic;
>>>>>>> c1cd01b62a225b343894011b6e778de45d58af63
#endregion

using KeybindingKV = System.Collections.Generic.KeyValuePair<string, Half_Caked.Keybinding[]>;
namespace Half_Caked
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            MenuEntry profileMenuEntry = new MenuEntry("Profile");
            MenuEntry resolutionMenuEntry = new MenuEntry("Resolution");
            MenuEntry soundMenuEntry = new MenuEntry("Sound");
            MenuEntry keybindingsMenuEntry = new MenuEntry("Keybindings");
            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            profileMenuEntry.Pressed += ProfileMenuEntrySelected;
            resolutionMenuEntry.Pressed += ResolutionMenuEntrySelected;
            soundMenuEntry.Pressed += SoundMenuEntrySelected;
            keybindingsMenuEntry.Pressed += KeybindingsMenuEntrySelected;
            backMenuEntry.Pressed += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(profileMenuEntry);
            MenuEntries.Add(resolutionMenuEntry);
            MenuEntries.Add(soundMenuEntry);
            MenuEntries.Add(keybindingsMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }

         #endregion

        #region Handle Input

        void ProfileMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            var device = (ScreenManager.Game as HalfCakedGame).Device;
            MessageBoxScreen msgbox;
            if (device != null)
                msgbox = new ProfileSelectionScreen(device);
            else
                msgbox = new MessageBoxScreen("Unable to write to your documents folder. Cannot save profiles.", new string[]{"Ok"}, 0);

            ScreenManager.AddScreen(msgbox, null);
        }

        void KeybindingsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            var scrn = new KeybindingsScreen((ScreenManager.Game as HalfCakedGame).CurrentProfile);

            ScreenManager.AddScreen(scrn, null);
        }

        void ResolutionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            var scrn = new GraphicsScreen((ScreenManager.Game as HalfCakedGame).CurrentProfile);

            ScreenManager.AddScreen(scrn, null);
        }
        
        void SoundMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            var scrn = new AudioOptionsScreen((ScreenManager.Game as HalfCakedGame).CurrentProfile);
            ScreenManager.AddScreen(scrn, null);
        }
        
        #endregion
    }

    class AudioOptionsScreen : MenuScreen
    {
        Slider masterVolumeSlider;
        Slider musicEffectSlider;
        Slider soundEffectSlider;
        Slider narrationVolumeSlider;

        public AudioOptionsScreen(Profile curProfile)
            : base("Audio Settings")
        {
            masterVolumeSlider = new Slider("Master Volume:", curProfile.Audio.MasterVolume);
            musicEffectSlider = new Slider("Music Volume:", curProfile.Audio.MusicVolume);
            soundEffectSlider = new Slider("Sound Effect Volume:", curProfile.Audio.SoundEffectsVolume);
            narrationVolumeSlider = new Slider("Narration Volume:", curProfile.Audio.NarrationVolume);
            MenuEntry saveMenuEntry = new MenuEntry("Save");
            MenuEntry backMenuEntry = new MenuEntry("Back");

            saveMenuEntry.Pressed += SaveButton;
            backMenuEntry.Pressed += OnCancel;

            MenuEntries.Add(masterVolumeSlider);
            MenuEntries.Add(musicEffectSlider);
            MenuEntries.Add(soundEffectSlider);
            MenuEntries.Add(narrationVolumeSlider);
            MenuEntries.Add(saveMenuEntry);
            MenuEntries.Add(backMenuEntry);

            mProfile = curProfile;
        }

        private Profile mProfile;

        void SaveButton(object sender, PlayerIndexEventArgs e)
        {
            mProfile.Audio.MasterVolume = masterVolumeSlider.Value;
            mProfile.Audio.MusicVolume = musicEffectSlider.Value;
            mProfile.Audio.SoundEffectsVolume = soundEffectSlider.Value;
            mProfile.Audio.NarrationVolume = narrationVolumeSlider.Value;

            string message;
            string[] prompt = { "Ok" };

            var device = (ScreenManager.Game as HalfCakedGame).Device;
            if (device != null)
            {
                Profile.SaveProfile(mProfile, "default.sav", device);
                SoundEffect.MasterVolume = mProfile.Audio.MasterVolume / 100f;
                MediaPlayer.Volume = mProfile.Audio.MasterVolume * mProfile.Audio.MusicVolume / 10000f;

                Console.WriteLine("Audio Settings Saved for " + mProfile.Name + "..." +
                    "\nMaster Volume: " + masterVolumeSlider.Value +
                    "\nMusic Effect Volume: " + musicEffectSlider.Value +
                    "\nSound Effect Volume: " + soundEffectSlider.Value +
                    "\nNarration Volume: " + narrationVolumeSlider.Value + "\n"
                );

                message = "Audio Settings Saved for " + mProfile.Name;
            }
            else
            {
                Console.WriteLine("Unable to write to documents folder. Cannot save audio settings.");
                message = "Unable to write to your documents folder. Cannot save audio settings.";
            }

            MessageBoxScreen savedMessageBox = new MessageBoxScreen(message, prompt, 0);

            ScreenManager.AddScreen(savedMessageBox, e.PlayerIndex);
        }
    }

    //This is just an example, the resolutions/display mode need to be made intelligable and the methods need to be implemented
    class GraphicsScreen : MenuScreen
    {
        public GraphicsScreen(Profile curProfile)
            : base("Graphics Settings")
        {
            // Create our menu entries.
            OptionPicker displayModeMenuEntry = new OptionPicker("Display Mode:", new string[3] { "W", "W (NB)", "FS" });
            OptionPicker resolutionMenuEntry = new OptionPicker( "Resolutions:", new string[3] { "A", "B", "C" });
            MenuEntry testMenuEntry = new MenuEntry("Test");
            MenuEntry saveMenuEntry = new MenuEntry("Save");
            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            testMenuEntry.Pressed += TestButton;
            saveMenuEntry.Pressed += SaveButton;
            backMenuEntry.Pressed += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(displayModeMenuEntry);
            MenuEntries.Add(resolutionMenuEntry);
            MenuEntries.Add(testMenuEntry);
            MenuEntries.Add(saveMenuEntry);
            MenuEntries.Add(backMenuEntry);

            mProfile = curProfile;
        }

        private Profile mProfile;

        void SaveButton(object sender, PlayerIndexEventArgs e) { }
        void TestButton(object sender, PlayerIndexEventArgs e) { }
    }

    class KeybindingsScreen : MenuScreen
    {
        private Profile mProfile;
        List<KeybindingKV> menuList;
        public KeybindingsScreen(Profile curProfile) : base("Keybindings") {
            mProfile = curProfile;

            // Creates the keybindings menu...
            menuList = new List<KeybindingKV>() {
                new KeybindingKV("Move Forward",        curProfile.KeyBindings.MoveForward){},
                new KeybindingKV("Move Backwards",      curProfile.KeyBindings.MoveBackwards){},
                new KeybindingKV("Crouch",              curProfile.KeyBindings.Crouch){},
                new KeybindingKV("Jump",                curProfile.KeyBindings.Jump){},
                new KeybindingKV("Interact",            curProfile.KeyBindings.Interact){},
                new KeybindingKV("Pause",               curProfile.KeyBindings.Pause){},
                new KeybindingKV("Portal (Entry) Fire", curProfile.KeyBindings.Portal1){},
                new KeybindingKV("Portal (Exit) Fire",  curProfile.KeyBindings.Portal2){},
            };
            
            foreach (KeybindingKV keyItem in menuList)
            {
                string title = keyItem.Key;
                string[] choices = new string[2];
                choices[0] = keyItem.Value[0].ToString();
                choices[1] = keyItem.Value[1].ToString();
                ButtonGroup buttonRow = new ButtonGroup(title, choices);
                buttonRow.Buttons[0].Pressed += OpenKeybindingDialog(keyItem, buttonRow, 0);
                buttonRow.Buttons[1].Pressed += OpenKeybindingDialog(keyItem, buttonRow, 1);
                MenuEntries.Add(buttonRow);
            }

            // Menu Items that are special
            MenuEntry acceptMenuEntry = new MenuEntry("Accept");
            MenuEntry cancelMenuEntry = new MenuEntry("Cancel");

            // Event bindings
            acceptMenuEntry.Pressed += SaveButton;
            cancelMenuEntry.Pressed += OnCancel;

            // Menu entries on our list
            MenuEntries.Add(acceptMenuEntry);
            MenuEntries.Add(cancelMenuEntry);
        }

        // Keybindings Dialog event generator
        System.EventHandler<PlayerIndexEventArgs> OpenKeybindingDialog(KeybindingKV s, ButtonGroup row, int index)
        {
            return (object sender, PlayerIndexEventArgs e) =>
            {
                MessageBoxScreen dialog = new KeybindingDialog(
                    s.Key,
                    (Keybinding input) => {
                        // update the user's profile with the new keybinding
                        this.SetKeybinding(s, input, row.SelectedButton);

                        //updates the GUI
                        row.Buttons[index].Text = input.ToString();
                        Resize(row.Buttons[index]);
                    }
                );
                ScreenManager.AddScreen(dialog, ControllingPlayer);
            };
        }
        public override void LoadContent()
        {
            base.LoadContent();

            int width = 0;
            foreach (UIElement btnGrp in MenuEntries)
                if(btnGrp is ButtonGroup)
                    width = (int)Math.Max(width, (btnGrp as ButtonGroup).ButtonWidth);

            foreach (UIElement btnGrp in MenuEntries)
                if (btnGrp is ButtonGroup)
                    (btnGrp as ButtonGroup).ButtonWidth = width;
        }
        
        private void Resize(Button newButton)
        {
            int width = (MenuEntries[0] as ButtonGroup).ButtonWidth;

            if (width < newButton.Size.X)
            {
                foreach (UIElement btnGrp in MenuEntries)
                    if (btnGrp is ButtonGroup)
                        (btnGrp as ButtonGroup).ButtonWidth = (int)newButton.Size.X;
            }
            else
                newButton.Widen(width - newButton.Size.X);
        }

        private void SetKeybinding(KeybindingKV s, Keybinding input, int whichBinding)
        {
            string displayName = s.Key;
            Keybinding[] key = s.Value;
            if (input == null) {
                throw new System.ArgumentNullException("Keybindings Menu returned null Keybinding object 'input'");
            }
            System.Console.Error.WriteLine("Request to set the {0} keybinding [{1}] to {2}", whichBinding, displayName, input.ToString());
            if (whichBinding < 0 || whichBinding > key.Length) {
                throw new System.IndexOutOfRangeException("Keybindings Menu tried to bind to a Keybinding index that doesn't exist.");
            }
            key[whichBinding] = input;
        }
        
        void SaveButton(object sender, PlayerIndexEventArgs e) {
            HalfCakedGame game = ScreenManager.Game as HalfCakedGame;
            Profile.SaveProfile(mProfile, "default.sav", game.Device);
            ExitScreen();
        }
    }
}

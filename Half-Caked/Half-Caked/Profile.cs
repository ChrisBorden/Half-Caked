using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Half_Caked
{
    public class Profile
    {
        private const int PROFILE_COUNT = 3;

        public int GlobalIdentifer = -1;
        public int ProfileNumber = 0;
        public string Name = "";
        public int CurrentLevel = 0;

        public Statistics[] LevelStatistics = new Statistics[Level.MAX_LEVELS];
        public AudioSettings Audio = new AudioSettings();
        public GraphicsSettings Graphics = new GraphicsSettings();
        public Keybindings KeyBindings = new Keybindings();

        public static void SaveProfile(Profile prof, string filename, StorageDevice device)
        {
            IAsyncResult result = device.BeginOpenContainer("Profiles", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            
            if (container.FileExists(filename))
                container.DeleteFile(filename);

            Stream stream = container.CreateFile(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(Profile));
            serializer.Serialize(stream, prof);

            stream.Close();
            container.Dispose();
        }

        public void Delete(StorageDevice device)
        {
            IAsyncResult result = device.BeginOpenContainer("Profiles", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            string filename = "profile" + ProfileNumber + ".sav";
            
            if (!container.FileExists(filename))
            {
                container.DeleteFile("default.sav");
                container.Dispose();
                return;
            }

            container.DeleteFile(filename);
            container.Dispose();
        }

        public void Register()
        {
            (new Thread(new ThreadStart(this.HelpRegister))).Start();
        }

        public static Profile Load(int number, StorageDevice device)
        {
            IAsyncResult result = device.BeginOpenContainer("Profiles", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            string filename;
            if (number >= 0)
                filename = "profile" + number + ".sav";
            else
                filename = "default.sav";

            if (!container.FileExists(filename))
            {
                container.Dispose();
                return null;
            }

            Stream stream = container.OpenFile(filename, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(Profile));
            Profile prof = serializer.Deserialize(stream) as Profile;

            stream.Close();
            container.Dispose();
            prof.MakeValid();
            return prof;
        }

        public static KeyValuePair<int, Profile[]> LoadAll(StorageDevice device)
        {
            Profile[] profArray = new Profile[PROFILE_COUNT];
            Profile defProf = Load(-1, device);
            int index = -1;
                        
            if(defProf != null && !defProf.Name.Equals(""))
            {
                for (int i = 0; i < PROFILE_COUNT; i++)
                {
                    profArray[i] = Load(i, device);
                    if (profArray[i] != null && profArray[i].Name.Equals(""))
                    {
                        profArray[i] = null;
                    }
                }
                profArray[defProf.ProfileNumber] = defProf;
                index = defProf.ProfileNumber;
            }

            return new KeyValuePair<int, Profile[]> (index, profArray);
        }

        /// <summary>
        /// </summary>
        public static void ChangeDefault(int oldDefaultProfile, int newDefaultProfile, StorageDevice device)
        {
            IAsyncResult result = device.BeginOpenContainer("Profiles", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            
            string oldDefaultName = "profile" + oldDefaultProfile + ".sav";
            string newDefaultName = "profile" + newDefaultProfile + ".sav";
            string defaultName = "default.sav";

            if (container.FileExists(defaultName) && container.FileExists(newDefaultName) && !container.FileExists(oldDefaultName))
            {
                Stream oldfile = container.OpenFile(defaultName, FileMode.Open);
                Stream newfile = container.CreateFile(oldDefaultName);
                oldfile.CopyTo(newfile);

                oldfile.Close();
                newfile.Close();
                container.DeleteFile(defaultName);

                oldfile = container.OpenFile(newDefaultName, FileMode.Open);
                newfile = container.CreateFile(defaultName);
                oldfile.CopyTo(newfile);

                oldfile.Close();
                newfile.Close();
                container.DeleteFile(newDefaultName);
            }

            container.Dispose();
        }

        private bool MakeValid()
        {
            bool madeChanges = false;

            this.CurrentLevel = (int)MathHelper.Clamp(this.CurrentLevel, 0, Level.MAX_LEVELS - 1);

            if (LevelStatistics == null)
            {
                this.LevelStatistics = new Statistics[Level.MAX_LEVELS];
                madeChanges = true;
            }

            if (LevelStatistics.Length < Level.MAX_LEVELS)
            {
                var stats = LevelStatistics.ToList();
                stats.AddRange(new Statistics[Level.MAX_LEVELS - LevelStatistics.Length]);
                LevelStatistics = stats.ToArray();

                madeChanges = true;
            }
            else if (LevelStatistics.Length > Level.MAX_LEVELS)
            {
                this.LevelStatistics = LevelStatistics.Take(Level.MAX_LEVELS).ToArray();
                madeChanges = true;
            }

            if (Name == null || this.Name.Length <= 0)
            {
                this.Name = " ";
                madeChanges = true;
            }

            if (this.Audio == null)
            {
                this.Audio = new AudioSettings();
                madeChanges = true;
            }

            if (this.Graphics == null)
            {
                this.Graphics = new GraphicsSettings();
                madeChanges = true;
            }

            if (this.KeyBindings == null)
            {
                this.KeyBindings = new Keybindings();
                madeChanges = true;
            }
            
            return !madeChanges;
        }

        private void HelpRegister()
        {
            this.GlobalIdentifer = Server.RegisterProfile(Name);
        }
    }

    [Serializable]
    public class Statistics
    {
        public DateTime Date;
        public double TimeElapsed;
        public int Deaths;
        public int PortalsOpened;
        public int Level;

        public int Score
        {
            get
            {
                return (int)TimeElapsed;
            }
        }

        public Statistics()
            : this(-1)
        {
        }

        public Statistics(int lvl)
        {
            Level = lvl;
        }

        public void UploadScore(int guid)
        {
            ThreadStart ts = delegate() { Server.SendHighScores(guid, this); };
            new Thread(ts).Start();
        }
    }

    [Serializable]
    public class GraphicsSettings
    {        
        public enum WindowType
        {
            Fullscreen,
            Window,
            WindowNoBorder
        }

        public WindowType PresentationMode = WindowType.Window;
        public Vector2 Resolution = new Vector2(1280, 768);
    }

    [Serializable]
    public class AudioSettings
    {
        public int MasterVolume = 100, MusicVolume = 100, SoundEffectsVolume = 100, NarrationVolume = 100;
        public bool Subtitles = true;
    }

    [Serializable]
    public class Keybindings
    {
        public Keybinding[] MoveForward =   { Keys.D, Keys.Right  };
        public Keybinding[] MoveBackwards = { Keys.A, Keys.Left   };
        public Keybinding[] Crouch =        { Keys.S, Keys.Down   };
        public Keybinding[] Jump =          { Keys.W, Keys.Up     };
        public Keybinding[] Interact =      { Keys.E, Keys.None   };
        public Keybinding[] Pause =         { Keys.P, Keys.Escape };
        public Keybinding[] Portal1 =       { 1, -1 };
        public Keybinding[] Portal2 =       { 2, -1 };            
    }
    
    public class Keybinding
    {
        public enum InputType
        {
            MouseClick,
            Key,
            Button,
            None
        }

        public InputType Type = InputType.None;
        public Keys Key;
        public int MouseClick;
        public Buttons Button;

        public override string ToString(){
            return  this.Type == InputType.Key          ? this.Key.ToString()
                : this.Type == InputType.MouseClick     ? this.MouseClick.ToString()
                : this.Type == InputType.Button         ? this.Button.ToString()
                :                                         "<Unknown Key>";
        }

        public static implicit operator Keybinding(Keys key)
        {
            Keybinding temp = new Keybinding();
            temp.Key = key;
            temp.Type = InputType.Key;
            return temp;
        }

        public static implicit operator Keybinding(int i)
        {
            Keybinding temp = new Keybinding();
            if (i < 1)
                return temp;

            temp.MouseClick = i;
            temp.Type = InputType.MouseClick;
            return temp;
        }

        public static implicit operator Keybinding(Buttons button)
        {
            Keybinding temp = new Keybinding();
            temp.Button = button;
            temp.Type = InputType.Button;
            return temp;
        }
    }
}

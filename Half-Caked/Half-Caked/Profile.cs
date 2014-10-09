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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Half_Caked
{
    public class Profile
    {
        public int GlobalIdentifer = -1;
        public int ProfileNumber = -1;
        public string Name = "";
        public int CurrentLevel = 0;

        public Statistics[] LevelStatistics = new Statistics[Level.INIT_LID_FOR_WORLD.Last()];
        public List<KeyValuePair<Guid, Statistics>> CustomLevelStatistics = new List<KeyValuePair<Guid, Statistics>>();
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

        public static KeyValuePair<int, List<Profile>> LoadAll(StorageDevice device)
        {
            List<Profile> profArray = new List<Profile>();
            Profile defProf = Load(-1, device);
            int index = -1;
                        
            IAsyncResult result = device.BeginOpenContainer("Profiles", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            
            XmlSerializer serializer = new XmlSerializer(typeof(Profile));

            foreach(string name in container.GetFileNames("*.sav"))
            {
                Stream stream = container.OpenFile(name, FileMode.Open);
                Profile prof = serializer.Deserialize(stream) as Profile;
                stream.Close();

                if (name.Equals("default.sav"))
                    index = prof.ProfileNumber;

                if (!prof.MakeValid())
                    Profile.SaveProfile(prof, name, device);
                
                profArray.Add(prof);
            }

            container.Dispose();

            return new KeyValuePair<int, List<Profile>> (index, profArray);
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
            else if (oldDefaultProfile < 0 && !container.FileExists(defaultName) && container.FileExists(newDefaultName))
            {
                Stream oldfile = container.OpenFile(newDefaultName, FileMode.Open);
                Stream newfile = container.CreateFile(defaultName);
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

            int maxLevels = Level.INIT_LID_FOR_WORLD.Last();
            this.CurrentLevel = (int)MathHelper.Clamp(this.CurrentLevel, 0, maxLevels);
            
            if (LevelStatistics == null)
            {
                this.LevelStatistics = new Statistics[maxLevels];
                madeChanges = true;
            }

            if (LevelStatistics.Length < maxLevels)
            {
                var stats = LevelStatistics.ToList();
                stats.AddRange(new Statistics[maxLevels - LevelStatistics.Length]);
                LevelStatistics = stats.ToArray();

                madeChanges = true;
            }
            else if (LevelStatistics.Length > maxLevels)
            {
                this.LevelStatistics = LevelStatistics.Take(maxLevels).ToArray();
                madeChanges = true;
            }

            if (Name == null || this.Name.Length <= 0)
            {
                this.Name = "No Namer";
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

            foreach(Statistics stats in LevelStatistics.Where(x => x != null))
                Server.SendHighScores(GlobalIdentifer, stats); 
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
                return (int) (10000/TimeElapsed);
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

        public Keybindings Clone(){
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (Keybindings) formatter.Deserialize(stream);
            }
        }

    }
    [Serializable]
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
                : this.Type == InputType.MouseClick     ? "Mouse #" + this.MouseClick.ToString()
                : this.Type == InputType.Button         ? this.Button.ToString()
                :                                         "<None>";
        }

        public static implicit operator Keybinding(Keys key)
        {
            Keybinding temp = new Keybinding();
            if (key == Keys.None) 
                return temp;

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

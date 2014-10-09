using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Half_Caked;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for PointControl.xaml
    /// </summary>
    public partial class ActionControl : UserControl
    {
        public class ActionControlModel : INotifyPropertyChanged
        {
            Obstacle mSwitch;
            Level mLevel;
            StackPanel mParent;
            ActionControl mControlParent;
            Obstacle mTarget;

            Half_Caked.KeyValuePair<Guid, int> mData;

            public ActionControlModel(ActionControl control, StackPanel parent, Level level, Obstacle obs)
            {
                mControlParent = control;
                mLevel = level;
                mSwitch = obs;
                mParent = parent;

                mData = new Half_Caked.KeyValuePair<Guid, int>(obs.Guid, 0);
                mTarget= Obstacles[0];
                mTarget.Actions.Add(mData);
            }

            public ActionControlModel(ActionControl control, StackPanel parent, Level level, Obstacle obs, Obstacle target)
            {
                mControlParent = control;
                mLevel = level;
                mSwitch = obs;
                mParent = parent;

                mData = target.Actions.Find(x => x.Key == obs.Guid);
                mTarget = target;
                obs.Actions.Add(mData);
            }

            public Half_Caked.KeyValuePair<Guid, int> Data
            {
                get { return mData; }
            }

            public int State
            {
                get { return mData.Value; }
                set {  mData.Value = value == -1 ? mData.Value : value; }
            }

            public List<Obstacle> Obstacles
            {
                get { return Level.Obstacles.Where(o => !o.Actions.Any(pair => pair.Key == mSwitch.Guid) || o.Guid == (mTarget == null ? Guid.Empty : mTarget.Guid)).ToList(); }
            }

            public Obstacle Target
            {
                get { return mTarget; }
                set 
                {
                    if (value == null)
                    {
                        mControlParent.RemoveButton_Click(this, new RoutedEventArgs());
                        return;
                    }

                    mTarget.Actions.Remove(Data);
                    mTarget = value;
                    mTarget.Actions.Add(Data);
                    OnPropertyChanged("States"); 
                }
            }
            
            public Level Level
            {
                get { return mLevel; }
            }

            public Obstacle Switch
            {
                get { return mSwitch; }
            }

            public StackPanel Parent
            {
                get { return mParent; }
            }

            public List<string> States 
            {
                get
                {
                    if (mTarget is Switch)
                        return Enum.GetNames(typeof(Switch.SwitchState)).ToList();
                    if (mTarget is Platform)
                        return Enum.GetNames(typeof(Platform.PlatformState)).ToList();
                    if (mTarget is Door)
                        return Enum.GetNames(typeof(Door.DoorState)).ToList();
                    return null;
                }
            }

            #region INotify Implementation
            public event PropertyChangedEventHandler PropertyChanged;

            // Create the OnPropertyChanged method to raise the event
            public void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
            #endregion  
        }

        ActionControlModel mData;

        public ActionControl(StackPanel parent, Obstacle obs, Level level)
            : base()
        {
            InitializeComponent();
            this.DataContext = mData = new ActionControlModel(this, parent, level, obs);
        }

        public ActionControl(StackPanel parent, Obstacle obs, Level level, Obstacle target)
            : base()
        {
            InitializeComponent();
            this.DataContext = mData = new ActionControlModel(this, parent, level, obs, target);
        }

        public void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            mData.Target.Actions.Remove(mData.Data);
            mData.Parent.Children.Remove(this);
        }
    }
}

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
using System.Windows.Shapes;
using System.ComponentModel;
using Half_Caked;
using Microsoft.Xna.Framework;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SwitchPropertiesWindow : PropertiesWindow
    {
        class SwitchModel : MovingModel
        {
            Switch mSwitch;
            
            public Switch Switch { get { return mSwitch; } }
            public Level Level { get { return mLevel; } }

            public override int X
            {
                get { return base.X; }
                set
                {
                    mSwitch.InitialPosition = new Vector2(value, mSwitch.InitialPosition.Y); base.X = value;
                }
            }

            public override int Y
            {
                get { return base.Y; }
                set
                {
                    mSwitch.InitialPosition = new Vector2(mSwitch.InitialPosition.X, value); base.Y = value;
                }
            }

            public int State
            {
                get { return mSwitch.InitialState; }
                set
                {
                    mSwitch.InitialState = value;
                    OnPropertyChanged("State");
                }
            }

            public SwitchModel(Switch mswitch, DesignerItem item, Level level)
                : base(item, level)
            {
                mSwitch = mswitch;

                if(!level.Obstacles.Contains(mSwitch))
                    level.Obstacles.Add(mSwitch);

                X = (int)mswitch.InitialPosition.X;
                Y = (int)mswitch.InitialPosition.Y;
            }

            public override List<UIElement> RemoveFromLevel()
            {
                mLevel.Obstacles.Remove(mSwitch);
                return base.RemoveFromLevel();
            }

            public void MakeNewAction(StackPanel sp)
            {
                sp.Children.Add(new ActionControl(sp, mSwitch, mLevel));      
            }
        }

        public SwitchPropertiesWindow(Switch mswitch, DesignerItem item, Level level)
            : base()
        {
            InitializeComponent();
            item.PropertyWindow = this;
            DataContext = new SwitchModel(mswitch, item, level);

            foreach(Obstacle target in level.Obstacles.Where(o => o.Actions.Any(pair => pair.Key == mswitch.Guid)))
                mTriggers.Children.Add(new ActionControl(mTriggers, mswitch, level, target));
 
            UpdatePossibleActions();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((DataContext as SwitchModel).Level.Obstacles.Count(o => !o.Actions.Any(pair => pair.Key == (DataContext as SwitchModel).Switch.Guid)) > 0)
                (DataContext as SwitchModel).MakeNewAction(mTriggers);
            UpdatePossibleActions();
        }

        public override void HandleSelection(object sender, BoolEventArgs e)
        {
            if (e.Value)
                UpdatePossibleActions();

            base.HandleSelection(sender, e);
        }

        private void mTriggers_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePossibleActions();
        }

        private void UpdatePossibleActions()
        {
            bool noErrors = false;
            while (!noErrors) 
                try
                {
                    foreach (ActionControl ac in mTriggers.Children.OfType<ActionControl>())
                        (ac.DataContext as ActionControl.ActionControlModel).OnPropertyChanged("Obstacles");
                    noErrors = true;
                }
                catch { }
        }
    }
}

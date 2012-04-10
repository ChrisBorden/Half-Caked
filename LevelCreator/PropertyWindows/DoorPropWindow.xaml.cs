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
using Microsoft.Xna.Framework;
using Half_Caked;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DoorPropertiesWindow
    {
        public DoorPropertiesWindow()
        {
            InitializeComponent();
        }
    }

    public class DoorModel : MovingModel
    {
        Door mDoor;
            
        public override int X
        {
            get { return base.X; }
            set
            {
                mDoor.InitialPosition = new Vector2(value, mDoor.InitialPosition.Y); base.X = value;
            }
        }

        public override int Y
        {
            get { return base.Y; }
            set
            {
                mDoor.InitialPosition = new Vector2(mDoor.InitialPosition.X, value); base.Y = value;
            }
        }

        public int State
        {
            get { return mDoor.InitialState; }
            set
            {
                mDoor.InitialState = value ;
                OnPropertyChanged("State");
            }
        }

        public bool Locked
        {
            get { return !mDoor.Actions.Any(X => X.Key == Character.CharacterGuid); }
            set
            {
                if(value)
                    mDoor.Actions.RemoveAll(x => x.Key == Character.CharacterGuid);
                else
                    mDoor.Actions.Add(new Half_Caked.KeyValuePair<Guid,int>(Character.CharacterGuid, (int)Door.DoorState.Opening));
            }
        }

        public DoorModel(Door door, DesignerItem item, Level level)
            : base(item, level)
        {
            Data = mDoor = door;

            if(!level.Obstacles.Contains(door))
                level.Obstacles.Add(door);
            X = (int)door.InitialPosition.X;
            Y = (int)door.InitialPosition.Y;
        }

        public override List<UIElement> RemoveFromLevel()
        {
            mLevel.Obstacles.Remove(mDoor);
            return base.RemoveFromLevel();
        }
    }
}

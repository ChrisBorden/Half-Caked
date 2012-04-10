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
using Half_Caked;
using Microsoft.Xna.Framework;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PlatformPropertiesWindow : PropertiesWindow
    {
        class PlatformModel : MovingModel
        {
            Platform mPlatform;
            Canvas mCanvas;
            List<DesignerItem> mChildren;

            public List<DesignerItem> Children
            {
                get { return mChildren; }
            }

            public override int X
            {
                get { return base.X; }
                set
                {
                    mPlatform.InitialPosition = new Vector2(value, mPlatform.InitialPosition.Y); base.X = value;
                }
            }

            public override int Y
            {
                get { return base.Y; }
                set
                {
                    mPlatform.InitialPosition = new Vector2(mPlatform.InitialPosition.X, value); base.Y = value;
                }
            }

            public int State
            {
                get { return mPlatform.InitialState; }
                set
                {
                    mPlatform.InitialState = value;
                    OnPropertyChanged("State");
                }
            }

            public int Speed
            {
                get { return (int)mPlatform.Speed; }
                set
                {
                    mPlatform.Speed = value;
                    OnPropertyChanged("State");
                }
            }

            public PlatformModel(PlatformPropertiesWindow ppw, Platform platform, Canvas canvas, DesignerItem item, Level level)
                : base(item, level)
            {
                Data = mPlatform = platform;
                mCanvas = canvas;

                if (!level.Obstacles.Contains(platform))
                {
                    level.Obstacles.Add(platform);
                }

                X = (int)platform.InitialPosition.X;
                Y = (int)platform.InitialPosition.Y;       

                mChildren = new List<DesignerItem>();
            }

            public override List<UIElement> RemoveFromLevel()
            {
                var list = base.RemoveFromLevel();
                list.AddRange(Children);

                mLevel.Obstacles.Remove(mPlatform);
                return list;
            }

            public void AddPoint(PlatformPropertiesWindow parent, int index)
            {
                if (index == -1)
                {
                    mPlatform.Path.Add(Vector2.Zero);
                    index = mPlatform.Path.Count;
                }

                PointControl pc = new PointControl(parent, mCanvas, index, mPlatform.Path);
                parent.mPoints.Children.Add(pc);

                pc.OrderChanged += (object sender, EventArgs e) =>
                {
                    int i = 0;
                    foreach (PointControl control in parent.mPoints.Children)
                        control.ChangeIndex(++i);
                };

                mChildren.Add((pc.DataContext as MovingModel).Item);

                int j = 0;
                foreach (PointControl control in parent.mPoints.Children)
                    control.ChangeIndex(++j);
            }
        }

        public PlatformPropertiesWindow(Platform platform, Canvas canvas, DesignerItem item, Level level)
        {
            InitializeComponent();
            item.PropertyWindow = this;
            DataContext = new PlatformModel(this, platform, canvas, item, level);

            for(int i = 1; i <= platform.Path.Count; i++)
                (DataContext as PlatformModel).AddPoint(this, i);
        }

        public override void HandleSelection(object sender, BoolEventArgs e)
        {
            base.HandleSelection(sender, e);

            foreach (DesignerItem item in (DataContext as PlatformModel).Children)
                item.Visibility = e.Value ? Visibility.Visible : Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            (DataContext as PlatformModel).AddPoint(this, -1);
        }
    }
}

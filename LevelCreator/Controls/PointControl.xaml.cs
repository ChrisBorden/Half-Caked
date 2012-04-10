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


namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for PointControl.xaml
    /// </summary>
    public partial class PointControl : UserControl
    {
        public event EventHandler OrderChanged;

        class PointControlModel : MovingModel
        {
            int mIndex;
            List<Vector2> mPoints;
            StackPanel mParent;
            Canvas mCanvas;
            TextBlock mLabel;

            public PointControlModel(StackPanel parent, Canvas canvas, List<Vector2> points, int indx, DesignerItem item, TextBlock txt)
                : base(item, null)
            {
                mCanvas = canvas;
                mPoints = points;
                mParent = parent;
                mLabel = txt;
                Index = indx;
            }

            public int Index
            {
                get { return mIndex; }
                set { mIndex = value - 1; mLabel.Text = "" + value; X = (int)mPoints[mIndex].X; Y = (int)mPoints[mIndex].Y; OnPropertyChanged("Index"); OnPropertyChanged("CanDown"); OnPropertyChanged("CanUp"); }
            }

            public override int X
            {
                get { return base.X; }
                set { mPoints[mIndex] = new Vector2(value, mPoints[mIndex].Y); base.X = value; }
            }

            public override int Y
            {
                get { return base.Y; }
                set { mPoints[mIndex] = new Vector2(mPoints[mIndex].X, value); base.Y = value; }
            }

            public List<Vector2> Points
            {
                get { return mPoints; }
            }

            public StackPanel Parent
            {
                get { return mParent; }
            }
            
            public Canvas Canvas
            {
                get { return mCanvas; }
            }
            
            public bool CanDown { get { return mIndex < mParent.Children.Count - 1; } }
            public bool CanUp { get { return mIndex > 0; } }
        }

        PointControlModel mData;
        List<Vector2> mPath;

        public PointControl(PlatformPropertiesWindow parent, Canvas canvas, int index, List<Vector2> path)
            : base()
        {
            InitializeComponent();
            mPath = path;

            DesignerItem pointIndicator;

            Ellipse rindicator = new Ellipse();
            rindicator.Fill = Brushes.SpringGreen;
            rindicator.Stroke = Brushes.Black;

            TextBlock txt = new TextBlock();
            txt.Text = "" + index;
            txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            Grid gr = new Grid();
            gr.Children.Add(rindicator);
            gr.Children.Add(txt);
            gr.IsHitTestVisible = false;

            pointIndicator = new DesignerItem();
            pointIndicator.CanResize = false;
            pointIndicator.Height = 15;
            pointIndicator.Width = 15;
            pointIndicator.Content = gr;
            pointIndicator.MinHeight = 2;
            pointIndicator.MinWidth = 2;

            Canvas.SetLeft(pointIndicator, path[index-1].X);
            Canvas.SetTop(pointIndicator, path[index-1].Y);
            Canvas.SetZIndex(pointIndicator, 1);

            canvas.Children.Add(pointIndicator);

            this.DataContext = pointIndicator.Model = mData = new PointControlModel(parent.mPoints, canvas, path, index, pointIndicator, txt);
            mData.Delete += RemoveItem;
            mData.Item.OnSelected += parent.HandleSelection;

            PointBlock.Text += index;
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            mData.Parent.Children.Remove(this);
            mData.Canvas.Children.Remove(mData.Item);
            mPath.RemoveAt(mData.Index);
            OrderChanged.Invoke(this, new EventArgs());
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveItem(sender, e);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            var temp = mData.Points[mData.Index - 1];
            mData.Points[mData.Index - 1] = mData.Points[mData.Index];
            mData.Points[mData.Index ] = temp;
            OrderChanged.Invoke(this, new EventArgs());
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            var temp = mData.Points[mData.Index];
            mData.Points[mData.Index] = mData.Points[mData.Index + 1];
            mData.Points[mData.Index + 1] = temp;
            OrderChanged.Invoke(this, new EventArgs());
        }

        public void ChangeIndex(int newIndex)
        {
            mData.Index = newIndex;
            PointBlock.Text = "Point " + newIndex;
        }
    }
}

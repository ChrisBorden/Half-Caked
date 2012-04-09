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
            Vector2 mPoint;
            StackPanel mParent;
            Canvas mCanvas;
            TextBlock mLabel;

            public PointControlModel(StackPanel parent, Canvas canvas, Vector2 pnt, int indx, DesignerItem item, TextBlock txt)
                : base(item, null)
            {
                mIndex = indx;
                mCanvas = canvas;
                mPoint = pnt;
                mParent = parent;
                mLabel = txt;
            }

            public int Index
            {
                get { return mIndex; }
                set { mIndex = value; mLabel.Text = "" + value;  OnPropertyChanged("Index"); OnPropertyChanged("CanDown"); OnPropertyChanged("CanUp"); }
            }

            public override int X
            {
                get { return base.X; }
                set { mPoint.X = value; base.X = value; }
            }

            public override int Y
            {
                get { return base.Y; }
                set { mPoint.Y = value; base.Y = value; }
            }

            public Vector2 Point
            {
                get { return mPoint; }
            }

            public StackPanel Parent
            {
                get { return mParent; }
            }
            
            public Canvas Canvas
            {
                get { return mCanvas; }
            }
            
            public bool CanDown { get { return mIndex < mParent.Children.Count; } }
            public bool CanUp { get { return mIndex > 1; } }
        }

        PointControlModel mData;
        List<Vector2> mPath;

        public PointControl(PlatformPropertiesWindow parent, Canvas canvas, Vector2 point, int index, List<Vector2> path)
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
            pointIndicator.Height = 5;
            pointIndicator.Width = 5;
            pointIndicator.Content = gr;

            Canvas.SetLeft(pointIndicator, point.X);
            Canvas.SetTop(pointIndicator, point.Y);
            Canvas.SetZIndex(pointIndicator, 1);

            canvas.Children.Add(pointIndicator);

            this.DataContext = pointIndicator.Model = mData = new PointControlModel(parent.mPoints, canvas, point, index, pointIndicator, txt);
            mData.Delete += RemoveItem;
            mData.Item.OnSelected += parent.HandleSelection;

            PointBlock.Text += index;
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            mData.Parent.Children.Remove(this);
            mData.Canvas.Children.Remove(mData.Item);
            mPath.Remove(mData.Point);
            OrderChanged.Invoke(this, new EventArgs());
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveItem(sender, e);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            var temp = mData.Parent.Children[mData.Index - 1];
            mData.Parent.Children.RemoveAt(mData.Index - 1);
            mData.Parent.Children.Insert(mData.Index - 2, temp);
            OrderChanged.Invoke(this, new EventArgs());
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            var temp = mData.Parent.Children[mData.Index - 1];
            mData.Parent.Children.RemoveAt(mData.Index - 1);
            mData.Parent.Children.Insert(mData.Index, temp);
            OrderChanged.Invoke(this, new EventArgs());
        }

        public void ChangeIndex(int newIndex)
        {
            mData.Index = newIndex;
            PointBlock.Text = "Point " + newIndex;
        }
    }
}

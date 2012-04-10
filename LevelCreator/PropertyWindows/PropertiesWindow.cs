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

namespace LevelCreator
{
    public class PropertiesWindow : ContentControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
          DependencyProperty.Register("Title", typeof(string),
                                      typeof(PropertiesWindow),
                                      new FrameworkPropertyMetadata("Properties Window"));

        public EventHandler<BoolEventArgs> SelectionHandler;
        
        public override void OnApplyTemplate()
        {
            if(SelectionHandler == null)
                SelectionHandler += new EventHandler<BoolEventArgs>(HandleSelection);
        }

        static PropertiesWindow()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertiesWindow), new FrameworkPropertyMetadata(typeof(PropertiesWindow)));
        }

        public PropertiesWindow()
        {
            SelectionHandler += new EventHandler<BoolEventArgs>(HandleSelection);
            this.SizeChanged += new SizeChangedEventHandler(PropertiesWindow_SizeChanged);
            Visibility = Visibility.Hidden;
        }

        void PropertiesWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Moved();
        }

        public virtual void HandleSelection(object sender, BoolEventArgs e)
        {
            Visibility = e.Value ? Visibility.Visible : Visibility = Visibility.Hidden;
            Canvas.SetZIndex(this, e.Value ? 1 : -1);
            Moved();
        }

        public virtual void Moved()
        {
            (DataContext as MovingModel).Moved();
            var canvas = this.Parent as Canvas;

            this.LayoutTransform = canvas.LayoutTransform.Inverse as Transform;

            if ((DataContext as MovingModel).Item.Width > canvas.Width * 3 / 4)
                Canvas.SetLeft(this, (DataContext as MovingModel).X + 5);            
            else if(canvas.Width / 2 > (DataContext as MovingModel).X)
                Canvas.SetLeft(this, (DataContext as MovingModel).Item.Width + (DataContext as MovingModel).X + 5);
            else if (ActualWidth != 0)
                Canvas.SetLeft(this, -this.ActualWidth / (canvas.LayoutTransform as ScaleTransform).ScaleX + (DataContext as MovingModel).X - 5);
            else
                Canvas.SetLeft(this, -200 / (canvas.LayoutTransform as ScaleTransform).ScaleX + (DataContext as MovingModel).X - 5);

            Canvas.SetTop(this, (DataContext as MovingModel).Y);
            if (canvas.Height / 2 > (DataContext as MovingModel).Y)
                Canvas.SetTop(this, (DataContext as MovingModel).Y);
            else if (ActualHeight != 0)
                Canvas.SetTop(this, -this.ActualHeight / (canvas.LayoutTransform as ScaleTransform).ScaleY + (DataContext as MovingModel).Y + (DataContext as MovingModel).Item.Height);
            else
                Canvas.SetTop(this, -200 / (canvas.LayoutTransform as ScaleTransform).ScaleY + (DataContext as MovingModel).Y + (DataContext as MovingModel).Item.Height);
        }
    }

    abstract public class MovingModel : INotifyPropertyChanged
    {
        protected DesignerItem mItem;
        protected Level mLevel;

        public DesignerItem Item { get { return mItem; } }
        public object Data;
        public event EventHandler Delete;

        public virtual int X
        {
            get { return (int)Canvas.GetLeft(mItem); }
            set
            {
                Canvas.SetLeft(mItem, value);
                OnPropertyChanged("X");
            }
        }

        public virtual int Y
        {
            get { return (int)Canvas.GetTop(mItem); }
            set
            {
                Canvas.SetTop(mItem, value);
                OnPropertyChanged("Y");
            }
        }

        public MovingModel(DesignerItem item, Level level)
        {
            mLevel = level;
            mItem = item;
        }

        public virtual void Moved()
        {
            X = X;
            Y = Y;
        }

        public virtual List<UIElement> RemoveFromLevel()
        {
            if (Delete != null)
                Delete.Invoke(this, new EventArgs());

            return new List<UIElement>();
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
}

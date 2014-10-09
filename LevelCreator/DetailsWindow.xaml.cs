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

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for ResizeWindow.xaml
    /// </summary>
    public partial class DetailsWindow : Window
    {
        bool mRevert = true;
        double mWidth, mHeight;
        float mGravity;
        string mName;

        class DetailsModel
        {
            public Level Level
            {
                get;
                set;
            }
            public Canvas Canvas
            {
                get;
                set;
            }

            public DetailsModel(Level l, Canvas c)
            {
                Level = l;
                Canvas = c;
            }
        }

        public DetailsWindow(Canvas canvas, Level level)
        {
            InitializeComponent();
            DataContext = new DetailsModel(level, canvas);
            mWidth = canvas.Width;
            mHeight = canvas.Height;
            mGravity = level.Gravity;
            mName = level.Name;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            mRevert = false;
            DialogResult = true;
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!mRevert)
                return;
            DialogResult = false;

            (DataContext as DetailsModel).Canvas.Width = mWidth;
            (DataContext as DetailsModel).Canvas.Height = mHeight;
            (DataContext as DetailsModel).Level.Gravity = mGravity;
            (DataContext as DetailsModel).Level.Name = mName;
        }
    }
}

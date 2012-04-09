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

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for ResizeWindow.xaml
    /// </summary>
    public partial class ResizeWindow : Window
    {
        bool mRevert = true;
        double mWidth, mHeight;

        public ResizeWindow(Canvas canvas)
        {
            InitializeComponent();
            DataContext = canvas;
            mWidth = canvas.Width;
            mHeight = canvas.Height;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            mRevert = false;
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

            (DataContext as Canvas).Width = mWidth;
            (DataContext as Canvas).Height = mHeight;
        }
    }
}

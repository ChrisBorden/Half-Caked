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
using System.IO;
using System.Windows.Markup;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter =
                "Images (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|" +
                "All files (*.*)|*.*";

            ofd.Title = "Select a Texture...";
            var result = ofd.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
                return;
                        
            BitmapImage src = new BitmapImage(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));
            ImageBrush brush = new ImageBrush(src);
            brush.TileMode = TileMode.Tile;
            brush.Viewport = new Rect(0, 0, src.Width, src.Height);
            brush.ViewportUnits = BrushMappingMode.Absolute;

            switch (Grid.GetColumn(sender as Button))
            {
                case 0:
                    (DataContext as Settings).AbsorbBrush = brush;
                    break;
                case 1:
                    (DataContext as Settings).NormalBrush = brush;
                    break;
                case 2:
                    (DataContext as Settings).ReflectBrush = brush;
                    break;
                case 3:
                    (DataContext as Settings).AmplifyBrush = brush;
                    break;
                case 4:
                    (DataContext as Settings).DeathBrush = brush;
                    break;
                case 5:
                    (DataContext as Settings).AntiPortalBrush = brush;
                    break;
                default:
                    return;
            }

            ((sender as Button).Content as Button).Background = brush;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (DataContext as Settings).Save();
            Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dr = System.Windows.Forms.MessageBox.Show("Are you sure you want to reset your current settings?", "Confirmation Dialog", System.Windows.Forms.MessageBoxButtons.YesNo);

            if(dr == System.Windows.Forms.DialogResult.Yes)
                (DataContext as Settings).Reset();
        }
    }

    public class Settings
    {
        public Brush AbsorbBrush { get; set; }
        public Brush NormalBrush { get; set; }
        public Brush AmplifyBrush { get; set; }
        public Brush ReflectBrush { get; set; }
        public Brush DeathBrush { get; set; }
        public Brush AntiPortalBrush { get; set; }

        public bool OverwriteBackground { get; set; }

        public Settings()
        {
            AbsorbBrush = Brushes.Red;
            NormalBrush = Brushes.Gray;
            AmplifyBrush = Brushes.Blue;
            ReflectBrush = Brushes.WhiteSmoke;
            DeathBrush = Brushes.Yellow;
            AntiPortalBrush = new SolidColorBrush(Color.FromArgb(100, 25, 100, 255));

            OverwriteBackground = true;
        }

        public void Reset()
        {
            AbsorbBrush = Brushes.Red;
            NormalBrush = Brushes.Gray;
            AmplifyBrush = Brushes.Blue;
            ReflectBrush = Brushes.WhiteSmoke;
            DeathBrush = Brushes.Yellow;
            AntiPortalBrush = new SolidColorBrush(Color.FromArgb(100, 25, 100, 255));

            OverwriteBackground = true;
        }

        public static Settings Load()
        {
            Settings settings = null;
            try
            {
                using (FileStream fs = new FileStream("settings.xml", FileMode.Open))
                    settings = XamlReader.Load(fs) as Settings;
            }
            catch { }

            return settings;
        }

        public void Save()
        {
            try
            {
                using (TextWriter textWriter = new StreamWriter("settings.xml"))
                    XamlWriter.Save(this, textWriter);
            }
            catch
            { }
        }
    }
}

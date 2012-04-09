using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Half_Caked;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MouseEventHandler ActiveMouseHandler;
        private GridLength mPrevCol = GridLength.Auto;
        private Level mLevel;
        private string mFileLocation;
        private bool mUnsavedWork = false;

        public MainWindow()
        {
            InitializeComponent();

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenCmdExecuted));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteCmdExecuted, DeleteCmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveCmdExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsCmdExecute));
            this.CommandBindings.Add(new CommandBinding(NavigationCommands.IncreaseZoom, ZoomIn, CanZoomIn));
            this.CommandBindings.Add(new CommandBinding(NavigationCommands.DecreaseZoom, ZoomOut, CanZoomOut));

            mLevel = new Level();
            MyDesignerCanvas.Level = mLevel;
        }

        #region Commands

        #region Delete Command

        void DeleteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var selected = MyDesignerCanvas.SelectedItems.Where(x => x.CanDelete).ToList();

            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i].Model != null)
                {
                    var list = selected[i].Model.RemoveFromLevel();
                    for (int j = 0; j < list.Count; j++)
                        MyDesignerCanvas.Children.Remove(list[j]);
                }

                MyDesignerCanvas.Children.Remove(selected[i]);
                MyDesignerCanvas.Children.Remove(selected[i].PropertyWindow);
            }
            MyDesignerCanvas.DeselectAll();
        }

        void DeleteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MyDesignerCanvas.SelectedItems.Count() > 0;
        }

        #endregion

        #region Open Command

        void OpenCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (mUnsavedWork)
                if (!ConfirmAction("You have currently unsaved work, opening another level will lose all your unsaved progress. Do you want to continue?"))
                    return;

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter =
                "XML (*.XML)|*.XML|" +
                "All files (*.*)|*.*";

            ofd.Title = "Select a Level...";
            var result = ofd.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            mLevel = LoadLevel(ofd.FileName);

            BitmapImage src = new BitmapImage(new Uri(ofd.FileName.Remove(ofd.FileName.Length - 3) + "png", UriKind.RelativeOrAbsolute));
            MyDesignerCanvas.Width = src.PixelWidth;
            MyDesignerCanvas.Height = src.PixelHeight;
            MyDesignerCanvas.Background = new ImageBrush(src);

            List<Image> gameImages = (ToolboxContainer.Content as Toolbox).Items.OfType<Image>().ToList();

            Image checkpointImage = gameImages.Find(x => x.ToolTip.Equals("Checkpoint"));
            Image enemyImage = gameImages.Find(x => x.ToolTip.Equals("Enemy"));
            Image platformImage = gameImages.Find(x => x.ToolTip.Equals("Platform"));
            Image switchImage = gameImages.Find(x => x.ToolTip.Equals("Switch"));
            Image doorImage = gameImages.Find(x => x.ToolTip.Equals("Door"));

            MyDesignerCanvas.Children.Clear();

            foreach (Tile t in mLevel.Tiles)
            {            
                Rectangle rect=  new Rectangle();
                rect.IsHitTestVisible = false;
                rect.Fill = Brushes.Gray;

                DesignerItem item = new DesignerItem();
                item.Content = rect;
                item.MinHeight = item.MinWidth = 1;
                MyDesignerCanvas.Children.Add(item);

                PropertiesWindow pw = new TilePropertiesWindow(t, item, mLevel);
                MyDesignerCanvas.Children.Add(pw);

                item.OnSelected += pw.SelectionHandler;
            }

            foreach (Enemy enemy in mLevel.Actors)
            {
                DesignerItem item = CreateDesignerImage(enemyImage);

                PropertiesWindow pw = new EnemyPropertiesWindow(enemy, MyDesignerCanvas, item, mLevel);
                MyDesignerCanvas.Children.Add(pw);

                item.OnSelected += pw.SelectionHandler;
            }

            foreach (Checkpoint cp in mLevel.Checkpoints)
            {
                DesignerItem item = CreateDesignerImage(checkpointImage);

                PropertiesWindow pw = new CheckpointPropertiesWindow(cp, MyDesignerCanvas, item, mLevel);
                MyDesignerCanvas.Children.Add(pw);

                item.OnSelected += pw.SelectionHandler;
            }

            foreach (Switch sw in mLevel.Obstacles.OfType<Switch>())
            {
                DesignerItem item = CreateDesignerImage(switchImage);

                PropertiesWindow pw = new SwitchPropertiesWindow(sw, item, mLevel);
                MyDesignerCanvas.Children.Add(pw);

                item.OnSelected += pw.SelectionHandler;
            }

            foreach (Door dr in mLevel.Obstacles.OfType<Door>())
            {
                DesignerItem item = CreateDesignerImage(doorImage);

                DoorModel dm = new DoorModel(dr, item, mLevel);
                PropertiesWindow pw = new DoorPropertiesWindow();
                pw.DataContext = dm;
                item.PropertyWindow = pw;
                MyDesignerCanvas.Children.Add(pw);

                item.OnSelected += pw.SelectionHandler;
            }

            foreach (Platform pltfrm in mLevel.Obstacles.OfType<Platform>())
            {
                DesignerItem item = CreateDesignerImage(platformImage);

                PropertiesWindow pw = new PlatformPropertiesWindow(pltfrm, MyDesignerCanvas, item, mLevel);
                MyDesignerCanvas.Children.Add(pw);
                item.OnSelected += pw.SelectionHandler;
            }

            MyDesignerCanvas.DeselectAll();
        }

        private static Level LoadLevel(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                XmlReader reader = new XmlTextReader(fs);

                Level lvl = (Level)serializer.Deserialize(reader);

                fs.Close();
                reader.Close();

                return lvl;
            }
            catch
            {
                return null;
            }
        }

        private DesignerItem CreateDesignerImage(Image source)
        {
            DesignerItem item = new DesignerItem();
            item.IsSelected = true;
            item.CanResize = false;
            Image img = new Image();
            img.IsHitTestVisible = false;
            img.Source = source.Source;
            item.Content = img;
            item.Width = img.Source.Width;
            item.Height = img.Source.Height;

            item.MinWidth = 2;
            item.MinHeight = 2;
                
            MyDesignerCanvas.Children.Add(item);
            return item;
        }

        #endregion

        #region Zoom Commands
        
        public void ZoomIn(object target, ExecutedRoutedEventArgs e)
        {
            this.Zoombox.ZoomIn();
        }

        public void CanZoomIn(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Zoombox.CanZoomIn();
        }

        public void ZoomOut(object target, ExecutedRoutedEventArgs e)
        {
            this.Zoombox.ZoomOut();
        }

        public void CanZoomOut(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Zoombox.CanZoomOut();
        }

        #endregion

        #region Save Commands

        void SaveCmdExecute(object target, ExecutedRoutedEventArgs e)
        {
            if (mFileLocation == null)
            {
                SaveAsCmdExecute(target, e);
                return;
            }

            SaveImage(mFileLocation);
            SaveLevel(mFileLocation);
        }

        void SaveAsCmdExecute(object target, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog();
            ofd.Filter =
                "All files (*.*)|*.*";

            ofd.Title = "Choose a Save Location...";
            var result = ofd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                mFileLocation = ofd.FileName;

                SaveImage(mFileLocation);
                SaveLevel(mFileLocation);
            }
        }

        private void SaveImage(string path)
        {
            MyDesignerCanvas.DeselectAll();

            FileStream fs = new FileStream(path + ".bmp", FileMode.Create);
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)MyDesignerCanvas.ActualWidth,
                (int)MyDesignerCanvas.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Pbgra32);

            var temp = MyDesignerCanvas.Background;
            MyDesignerCanvas.Background = Brushes.Transparent;

            bmp.Render(MyDesignerCanvas);
            BitmapEncoder encoder = new TiffBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(fs);
            fs.Close();

            MyDesignerCanvas.Background = temp;
        }

        private void SaveLevel(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            TextWriter textWriter = new StreamWriter(path + ".xml");
            serializer.Serialize(textWriter, mLevel);
            textWriter.Close(); 
        }

        #endregion

        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Col1.MinWidth = 90;
            Col1.Width = mPrevCol;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Col1.MinWidth = 25;
            mPrevCol = Col1.Width;
            Col1.Width = GridLength.Auto;
        }

        private void SelectBackground_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();   
            ofd.Filter =
                "Images (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|" +
                "All files (*.*)|*.*";

            ofd.Title = "Select a Background...";
            var result = ofd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {            
                BitmapImage src = new BitmapImage(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));
                MyDesignerCanvas.Background = new ImageBrush(src);
            }
        }

        private bool ConfirmAction(string s)
        {
            var dr = System.Windows.Forms.MessageBox.Show(s, "Confirmation Dialog", System.Windows.Forms.MessageBoxButtons.YesNo);

            return dr == System.Windows.Forms.DialogResult.Yes;
        }

        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResizeLevel_Click(object sender, RoutedEventArgs e)
        {
            ResizeWindow rw = new ResizeWindow(MyDesignerCanvas);
            rw.ShowDialog();
        }
    }
}

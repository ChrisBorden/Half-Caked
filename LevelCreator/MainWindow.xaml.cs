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
        Image mCheckpointImage, mEnemyImage, mPlatformImage, mSwitchImage, mDoorImage;

        private GridLength mPrevCol = GridLength.Auto;
        private Level mLevel;

        public Level Level
        {
            get { return mLevel; }
            set { mLevel = value; MyDesignerCanvas.Level = mLevel; }
        }
        
        private string mFileLocation;
        private bool mUnsavedWork = false, mFirstSave = true;
        private List<object> mClipboard; // using custom clipboard because kept running into OOM exceptions

        public MainWindow()
        {
            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(MainWindow), UIElement.PreviewKeyDownEvent, new KeyEventHandler(KeyDownHandler));

            List<Image> gameImages = (ToolboxContainer.Content as Toolbox).Items.OfType<Image>().ToList();

            mCheckpointImage = gameImages.Find(x => x.ToolTip.Equals("Checkpoint"));
            mEnemyImage = gameImages.Find(x => x.ToolTip.Equals("Enemy"));
            mPlatformImage = gameImages.Find(x => x.ToolTip.Equals("Platform"));
            mSwitchImage = gameImages.Find(x => x.ToolTip.Equals("Switch"));
            mDoorImage = gameImages.Find(x => x.ToolTip.Equals("Door"));

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenCmdExecuted));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteCmdExecuted, DeleteCmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, CutCmdExecuted, CopyCmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, PasteCmdExecuted, PasteCmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveCmdExecuted));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsCmdExecuted));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewCmdExecuted));
            this.CommandBindings.Add(new CommandBinding(NavigationCommands.IncreaseZoom, ZoomIn, CanZoomIn));
            this.CommandBindings.Add(new CommandBinding(NavigationCommands.DecreaseZoom, ZoomOut, CanZoomOut));
            this.CommandBindings.Add(new CommandBinding(NavigationCommands.Favorites, OptionsCmdExecuted));
        }

        #region Commands

        #region Options Command

        void OptionsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            OptionsWindow ow = new OptionsWindow();
            ow.ShowDialog();
        }

        #endregion

        #region Delete Command

        void DeleteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var selected = MyDesignerCanvas.SelectedItems.Where(x => x.CanDelete).ToList();

            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i].PropertyWindow != null)
                {
                    var list = (selected[i].PropertyWindow.DataContext as MovingModel).RemoveFromLevel();
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

        #region New Command

        void NewCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (mUnsavedWork)
                if (!ConfirmAction("You have currently unsaved work. Creating a new level will cause all your unsaved progress to be lost. Do you wish to continue?"))
                    return;

            Level lvl = new Level();
            lvl.CustomLevelIdentifier = Guid.NewGuid();
            Canvas c = new Canvas();
            c.Width = 2000;
            c.Height = 1500;

            DetailsWindow dw = new DetailsWindow(c, lvl);
            if (dw.ShowDialog() != true)
                return;

            mFileLocation = null;
            mFirstSave = true;

            MyDesignerCanvas.Children.Clear();
            MyDesignerCanvas.Background = Brushes.Transparent;
            MyDesignerCanvas.Width  = c.Width;
            MyDesignerCanvas.Height = c.Height;
            MyDesignerCanvas.IsEnabled = true;
            MainGrid.Visibility = Visibility.Visible;
            Level = lvl;
        }

        #endregion

        #region Open Command

        void OpenCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (mUnsavedWork)
                if (!ConfirmAction("You have currently unsaved work. Opening a level will cause all your unsaved progress to be lost. Do you wish to continue?"))
                    return;

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter =
                "XML (*.XML)|*.XML|" +
                "All files (*.*)|*.*";

            ofd.Title = "Select a Level...";
            var result = ofd.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            Level = LoadLevel(ofd.FileName);

            BitmapImage src = new BitmapImage(new Uri(ofd.FileName.Remove(ofd.FileName.Length - 3) + "png", UriKind.RelativeOrAbsolute));
            MyDesignerCanvas.Width = src.PixelWidth;
            MyDesignerCanvas.Height = src.PixelHeight;
            
            MyDesignerCanvas.Children.Clear();

            foreach (Tile t in Level.Tiles)
                AddTile(t);

            foreach (Enemy enemy in Level.Actors)
                AddEnemy(enemy);

            foreach (Checkpoint cp in Level.Checkpoints)
                AddCheckpoint(cp);

            foreach (Obstacle obs in Level.Obstacles)
            {
                if(obs is Switch)
                    AddSwitch(obs as Switch);
                else if (obs is Door)
                    AddDoor(obs as Door);
                else if (obs is Platform)
                    AddPlatform(obs as Platform);
            }

            MyDesignerCanvas.DeselectAll();

            mFirstSave = false;
            MyDesignerCanvas.IsEnabled = true;
            MainGrid.Visibility = Visibility.Visible;
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

        void SaveCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (mFileLocation == null)
            {
                SaveAsCmdExecuted(target, e);
                return;
            }
            
            try
            {
                SaveImage(mFileLocation);
                SaveLevel(mFileLocation);
                mFirstSave = false;
            }
            catch
            {
                MessageBox.Show("Unable to Save your level, an unexpected error occured.");
            }
        }

        void SaveAsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog();
            ofd.Filter =
                "All files (*.*)|*.*";

            ofd.Title = "Choose a Save Location...";
            var result = ofd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                mFileLocation = ofd.FileName;
                Level.CustomLevelIdentifier = Guid.NewGuid();

                try
                {
                    SaveImage(mFileLocation);
                    SaveLevel(mFileLocation);
                }
                catch
                {
                    MessageBox.Show("Unable to Save your level, an unexpected error occured.");
                }

                mFirstSave = false;
            }
        }

        private void SaveImage(string path)
        {
            MyDesignerCanvas.DeselectAll();
            Zoombox.ZoomTo(100);

            var temp = MyDesignerCanvas.Background;

            FileStream fs = new FileStream(path + "b.png", FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            var originalBitmap = (temp as ImageBrush).ImageSource as BitmapSource; 
            var resizedBitmap = new TransformedBitmap(originalBitmap, new ScaleTransform(MyDesignerCanvas.ActualWidth/originalBitmap.Width, MyDesignerCanvas.ActualHeight/originalBitmap.Height));

            encoder.Frames.Add(BitmapFrame.Create(resizedBitmap));
            encoder.Save(fs);
            fs.Close();

            MyDesignerCanvas.Background = Brushes.Transparent;

            foreach (DesignerItem item in MyDesignerCanvas.Children.OfType<DesignerItem>())
                item.Visibility = item.Content is Rectangle ? item.Visibility : Visibility.Hidden;

            MyDesignerCanvas.UpdateLayout();

            fs = new FileStream(path + ".png", FileMode.Create);
            RenderTargetBitmap png = new RenderTargetBitmap((int)MyDesignerCanvas.ActualWidth,
                (int)MyDesignerCanvas.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Pbgra32);

            png.Render(MyDesignerCanvas);
            encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(png));
            encoder.Save(fs);

            fs.Close();

            MyDesignerCanvas.Background = temp;

            foreach (DesignerItem item in MyDesignerCanvas.Children.OfType<DesignerItem>())
                item.Visibility = Visibility.Visible;
            foreach (DesignerItem item in MyDesignerCanvas.Children.OfType<DesignerItem>())
                item.IsSelected = true;
            MyDesignerCanvas.DeselectAll();
        }

        private void SaveLevel(string path)
        {
            Tile[] boundaries = null;
            if (mFirstSave)
            {
                boundaries = new Tile[]{ new Tile(new Microsoft.Xna.Framework.Rectangle(-2, 0, 2,(int) MyDesignerCanvas.ActualHeight), Surface.Absorbs), 
                                            new Tile(new Microsoft.Xna.Framework.Rectangle(0, -2, (int) MyDesignerCanvas.ActualWidth, 2), Surface.Absorbs),    
                                            new Tile(new Microsoft.Xna.Framework.Rectangle(0, (int) MyDesignerCanvas.ActualHeight + 1, (int) MyDesignerCanvas.ActualWidth, 2), Surface.Absorbs), 
                                            new Tile(new Microsoft.Xna.Framework.Rectangle((int) MyDesignerCanvas.ActualWidth + 1, 0, 2, (int) MyDesignerCanvas.ActualHeight), Surface.Absorbs) };

                Level.Tiles.AddRange(boundaries);
            }

            Level.AssetName = path.Substring(path.LastIndexOf('\\') + 1);

            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            TextWriter textWriter = new StreamWriter(path + ".xml");
            serializer.Serialize(textWriter, Level);

            textWriter.Close();

            if (mFirstSave)
                Level.Tiles.RemoveAll(t => boundaries.Contains(t));
            mFirstSave = false;
        }

        #endregion
        
        #region Copy Cmmand

        void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            mClipboard = new List<object>();
            var selected = MyDesignerCanvas.SelectedItems.Where(x => x.PropertyWindow != null && (x.PropertyWindow.DataContext as MovingModel).Data != null).Select(x => (x.PropertyWindow.DataContext as MovingModel).Data).ToList();

            foreach(object obj in selected)
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                using (MemoryStream mem = new MemoryStream())
                {
                    serializer.Serialize(mem, obj);
                    mem.Position = 0;
                    mClipboard.Add(serializer.Deserialize(mem));           
                }
            }
        }

        void CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MyDesignerCanvas.SelectedItems.Count() > 0;
        }

        #endregion

        #region Cut Cmmand

        void CutCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            CopyCmdExecuted(target, e);
            DeleteCmdExecuted(target, e);
        }
        
        #endregion

        #region Paste Command

        void PasteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            foreach (object obj in mClipboard)
            {
                switch (obj.GetType().Name)
                {
                    case "Tile":
                        AddTile(obj as Tile);
                        break;
                    case "Checkpoint":
                        AddCheckpoint(obj as Checkpoint);
                        break;
                    case "Door":
                        (obj as Obstacle).Guid = Guid.NewGuid();
                        AddDoor(obj as Door);
                        break;
                    case "Switch":
                        (obj as Obstacle).Guid = Guid.NewGuid();
                        AddSwitch(obj as Switch);
                        break;
                    case "Platform":
                        (obj as Obstacle).Guid = Guid.NewGuid();
                        AddPlatform(obj as Platform);
                        break;
                    case "Enemy":
                        AddEnemy(obj as Enemy);
                        break;
                    default:
                        break;
                }
            }
        }

        void PasteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mClipboard != null && mClipboard.Count > 0;
        }

        #endregion

        #region Level MenuItem Commands

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

        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            DetailsWindow rw = new DetailsWindow(MyDesignerCanvas, Level);
            rw.ShowDialog();
        }

        #endregion

        #endregion

        #region Helper Item Methods

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

        private void AddCheckpoint(Checkpoint cp)
        {
            DesignerItem item = CreateDesignerImage(mCheckpointImage);

            PropertiesWindow pw = new CheckpointPropertiesWindow(cp, MyDesignerCanvas, item, Level);
            MyDesignerCanvas.Children.Add(pw);

            item.OnSelected += pw.SelectionHandler;
            item.IsSelected = true;
        }

        private void AddEnemy(Enemy enemy)
        {
            DesignerItem item = CreateDesignerImage(mEnemyImage);

            PropertiesWindow pw = new EnemyPropertiesWindow(enemy, MyDesignerCanvas, item, Level);
            MyDesignerCanvas.Children.Add(pw);

            item.OnSelected += pw.SelectionHandler;
            item.IsSelected = true;
        }

        private void AddDoor(Door door)
        {
            DesignerItem item = CreateDesignerImage(mDoorImage);

            DoorModel dm = new DoorModel(door, item, Level);
            PropertiesWindow pw = new DoorPropertiesWindow();
            pw.DataContext = dm;
            item.PropertyWindow = pw;
            MyDesignerCanvas.Children.Add(pw);

            item.OnSelected += pw.SelectionHandler;
            item.IsSelected = true;
        }

        private void AddSwitch(Switch sw)
        {
            DesignerItem item = CreateDesignerImage(mSwitchImage);

            PropertiesWindow pw = new SwitchPropertiesWindow(sw, item, Level);
            MyDesignerCanvas.Children.Add(pw);

            item.OnSelected += pw.SelectionHandler;
            item.IsSelected = true;
        }

        private void AddPlatform(Platform pltfrm)
        {
            DesignerItem item = CreateDesignerImage(mPlatformImage);

            PropertiesWindow pw = new PlatformPropertiesWindow(pltfrm, MyDesignerCanvas, item, Level);
            MyDesignerCanvas.Children.Add(pw);
            item.OnSelected += pw.SelectionHandler;
            item.IsSelected = true;
        }
        
        private void AddTile(Tile t)
        {
            Rectangle rect = new Rectangle();
            rect.IsHitTestVisible = false;
            rect.Fill = Brushes.Gray;

            DesignerItem item = new DesignerItem();
            item.Content = rect;
            item.MinHeight = item.MinWidth = 1;
            MyDesignerCanvas.Children.Add(item);

            PropertiesWindow pw = new TilePropertiesWindow(t, item, Level);
            MyDesignerCanvas.Children.Add(pw);

            item.OnSelected += pw.SelectionHandler;
            item.IsSelected = true;
        }
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
        
        private bool ConfirmAction(string s)
        {
            var dr = System.Windows.Forms.MessageBox.Show(s, "Confirmation Dialog", System.Windows.Forms.MessageBoxButtons.YesNo);

            return dr == System.Windows.Forms.DialogResult.Yes;
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (MyDesignerCanvas.SelectedItems.Count() > 0)
            {
                int deltaX = 0, deltaY = 0;

                switch (e.Key)
                {
                    case Key.Left: deltaX--; break;
                    case Key.Right: deltaX++; break;
                    case Key.Up: deltaY--; break;
                    case Key.Down: deltaY++; break;
                    default: return;
                }

                foreach (DesignerItem item in MyDesignerCanvas.SelectedItems)
                {
                    Canvas.SetLeft(item, Math.Max(Math.Min(Canvas.GetLeft(item) + deltaX, MyDesignerCanvas.ActualWidth - item.Width), 0));
                    Canvas.SetTop(item, Math.Max(Math.Min(Canvas.GetTop(item) + deltaY, MyDesignerCanvas.ActualHeight - item.Height), 0));

                    if (item.PropertyWindow != null)
                        item.PropertyWindow.Moved();
                    else if (item.Model != null)
                        item.Model.Moved();
                }

                e.Handled = true;
            }
        }
    }
}

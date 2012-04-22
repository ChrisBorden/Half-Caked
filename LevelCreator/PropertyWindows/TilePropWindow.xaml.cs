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
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TilePropertiesWindow : PropertiesWindow
    {
        class TileModel : MovingModel
        {
            Tile mTile;

            SolidColorBrush antiportalColor = new SolidColorBrush(Color.FromArgb(100, 25, 100, 255));

            public new double X
            {
                get { return (Canvas.GetLeft(mItem) + (IsCentered ? (mItem.Width / 2) : 0)); }
                set
                {
                    mTile.Dimensions.X = (int)value; 
                    Canvas.SetLeft(mItem, value - (IsCentered ? (mItem.Width / 2) : 0));
                    OnPropertyChanged("X");
                }
            }

            public new double Y
            {
                get { return (Canvas.GetTop(mItem) + (IsCentered ? (mItem.Height / 2) : 0)); }
                set
                {
                    mTile.Dimensions.Y = (int)value; 
                    Canvas.SetTop(mItem, value - (IsCentered ? (mItem.Height / 2) : 0));
                    OnPropertyChanged("Y");
                }
            }

            public double Width
            {
                get { return Item.Width; }
                set
                {
                    mTile.Dimensions.Width = (int)value;
                    mItem.Width = value;
                    OnPropertyChanged("Width");
                }
            }

            public double Height
            {
                get { return mItem.Height; }
                set
                {
                    mTile.Dimensions.Height = (int)value;
                    mItem.Height = value;
                    OnPropertyChanged("Height");
                }
            }

            public Surface Type
            {
                get { return mTile.Type; }
                set
                {
                    mTile.Type = value;
                    switch (value)
                    {
                        case Surface.Absorbs:
                            (mItem.Content as Rectangle).Fill = Brushes.Red;
                            break;
                        case Surface.Amplifies:
                            (mItem.Content as Rectangle).Fill = Brushes.Blue;
                            break;
                        case Surface.Death:
                            (mItem.Content as Rectangle).Fill = Brushes.Yellow;
                            break;
                        case Surface.Normal:
                            (mItem.Content as Rectangle).Fill = Brushes.Gray;
                            break;
                        case Surface.Reflects:
                            (mItem.Content as Rectangle).Fill = Brushes.GhostWhite;
                            break;
                        case Surface.Antiportal:
                            (mItem.Content as Rectangle).Fill = antiportalColor;
                            break;
                        default:
                            break;
                    }
                    OnPropertyChanged("Type");
                }
            }

            public TileModel(Tile tile, DesignerItem item, Level level)
                : base(item, level)
            {
                Data = mTile = tile;
                
                if(!level.Tiles.Contains(mTile))
                    level.Tiles.Add(mTile);

                Type = Type;
                Width = tile.Dimensions.Width;
                Height = tile.Dimensions.Height;
                X = tile.Dimensions.X;
                Y = tile.Dimensions.Y;
            }

            public override void Moved()
            {
                X = X; Y = Y;
            }

            public override List<UIElement> RemoveFromLevel()
            {
                mLevel.Tiles.Remove(mTile);
                return base.RemoveFromLevel();
            }
        }

        public TilePropertiesWindow(Tile tile, DesignerItem item, Level level)
            : base()
        {
            InitializeComponent();
            item.PropertyWindow = this;
            DataContext = new TileModel(tile, item, level);
            item.SizeChanged += new SizeChangedEventHandler(item_SizeChanged);
        }

        void item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var mdl = (this.DataContext as TileModel);

            mdl.Width = mdl.Width;
            mdl.Height = mdl.Height;
            Moved();
        }
    }
}

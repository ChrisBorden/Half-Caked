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

            public override int X
            {
                get { return base.X; }
                set
                {
                    mTile.Dimensions.X = value; base.X = value;
                }
            }

            public override int Y
            {
                get { return base.Y; }
                set
                {
                    mTile.Dimensions.Y = value; base.Y = value;
                }
            }

            public int Width
            {
                get { return (int)mItem.Width; }
                set
                {
                    mTile.Dimensions.Width = value;
                    mItem.Width = value;
                }
            }

            public int Height
            {
                get { return (int)mItem.Height; }
                set
                {
                    mTile.Dimensions.Height = value;
                    mItem.Height = value;
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
                            (mItem.Content as Rectangle).Fill = Brushes.WhiteSmoke;
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
                mTile = tile;
                
                if(!level.Tiles.Contains(mTile))
                    level.Tiles.Add(mTile);

                Type = Type;
                Width = tile.Dimensions.Width;
                Height = tile.Dimensions.Height;
                X = tile.Dimensions.X;
                Y = tile.Dimensions.Y;
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
            (this.DataContext as TileModel).OnPropertyChanged("Width");
            (this.DataContext as TileModel).OnPropertyChanged("Height");
            Moved();
        }
    }
}

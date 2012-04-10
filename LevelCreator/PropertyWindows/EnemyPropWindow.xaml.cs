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
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Half_Caked;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class EnemyPropertiesWindow : PropertiesWindow
    {
        public EnemyPropertiesWindow(Enemy enemy, DesignerCanvas canvas, DesignerItem item, Level level)
            : base()
        {
            InitializeComponent();
            item.PropertyWindow = this;

            Ellipse rangeFinder = new Ellipse();
            Canvas.SetZIndex(rangeFinder, -10);

            var fadedRed = System.Windows.Media.Color.FromArgb(0, 255, 0, 50);

            rangeFinder.Fill = new RadialGradientBrush(System.Windows.Media.Colors.Red, fadedRed);
            rangeFinder.Stroke = Brushes.Green;
            rangeFinder.IsHitTestVisible = false;

            canvas.Children.Add(rangeFinder);

            DataContext = new EnemyModel(enemy, rangeFinder, item, level);
        }

        public override void HandleSelection(object sender, BoolEventArgs e)
        {
 	        base.HandleSelection(sender, e);
            (DataContext as EnemyModel).RangeFinder.Visibility = e.Value ? Visibility.Visible : Visibility.Hidden;
        }
    }

    class EnemyModel : MovingModel
    {
        Enemy mEnemy;
        Ellipse mRangeFinder;

        public Ellipse RangeFinder  { get { return mRangeFinder; } }
        
        public override int X
        {
            get { return base.X; }
            set
            {
                Canvas.SetLeft(mRangeFinder, value + mItem.Width / 2 - mEnemy.Range);
                mEnemy.InitialPosition = new Vector2(value, mEnemy.InitialPosition.Y); base.X = value;
            }
        }

        public override int Y
        {
            get { return base.Y; }
            set
            {
                Canvas.SetTop(mRangeFinder, value + mItem.Height / 2 - mEnemy.Range);
                mEnemy.InitialPosition = new Vector2(mEnemy.InitialPosition.X, value); base.Y = value;
            }
        }

        public int Range
        {
            get { return mEnemy.Range;      }
            set
            {
                mRangeFinder.Width = value * 2;
                mRangeFinder.Height = value * 2;
                Canvas.SetLeft(mRangeFinder, X + mItem.Width / 2 - value);
                Canvas.SetTop(mRangeFinder, Y + mItem.Height / 2 - value);

                mEnemy.Range = value;
                OnPropertyChanged("Range");
            }
        }

        public EnemyModel(Enemy enemy, Ellipse rangeFinder, DesignerItem item, Level level)
            : base(item, level)
        {
            Data = mEnemy = enemy;
            mRangeFinder = rangeFinder;

            if(!level.Actors.Contains(mEnemy))
                level.Actors.Add(mEnemy);

            X = (int)enemy.InitialPosition.X;
            Y = (int)enemy.InitialPosition.Y;
            Range = Range;
        }

        public override List<UIElement> RemoveFromLevel()
        {
            var list = base.RemoveFromLevel();
            list.Add(mRangeFinder);

            mLevel.Actors.Remove(mEnemy);
            return list;
        }
    }

}

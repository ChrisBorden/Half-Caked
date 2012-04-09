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
using Microsoft.Xna.Framework;

namespace LevelCreator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CheckpointPropertiesWindow : PropertiesWindow
    {
        class CheckpointModel : MovingModel
        {
            Checkpoint mCheckpoint;
            System.Windows.Shapes.Rectangle mCheckpointRegion;
            DesignerItem mRespawnIndicator;
            Canvas mCanvas;

            public List<UIElement> Children
            {
                get { return new List<UIElement>() { mRespawnIndicator, mCheckpointRegion }; }
            }

            public DesignerItem RespawnIndicator
            {
                get { return mRespawnIndicator; }
            }

            public override int X
            {
                get { return base.X; }
                set
                {
                    mCheckpoint.Bound.X = base.X = value;
                    Quadrant = Quadrant;
                }
            }

            public override int Y
            {
                get { return base.Y; }
                set
                {
                    mCheckpoint.Bound.Y = base.Y = value;
                    Quadrant = Quadrant;
                }
            }

            public int Quadrant
            {
                get 
                {
                    if (mCheckpoint.Quadrant.X < 0)
                    {
                        if (mCheckpoint.Quadrant.Y < 0)
                        {
                            return 3;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        if (mCheckpoint.Quadrant.Y < 0)
                        {
                            return 2;
                        }
                        else
                        {
                            return 1;
                        }
                    }
                }
                set
                {
                    mCheckpoint.Quadrant = new Vector2(value % 3 == 0 ? -1 : 1, value > 1 ? -1 : 1);

                    if (mCheckpoint.Quadrant.X > 0)
                    {
                        Canvas.SetLeft(mCheckpointRegion, 0);
                        mCheckpointRegion.Width = Math.Max(X, 0);
                    }
                    else
                    {
                        Canvas.SetLeft(mCheckpointRegion, X);
                        mCheckpointRegion.Width = mCanvas.Width - X;
                    }

                    if (mCheckpoint.Quadrant.Y > 0)
                    {
                        Canvas.SetTop(mCheckpointRegion, 0);
                        mCheckpointRegion.Height = Math.Max(Y,0);
                    }
                    else
                    {
                        Canvas.SetTop(mCheckpointRegion, Y);
                        mCheckpointRegion.Height = mCanvas.Height - Y;
                    }

                    OnPropertyChanged("Quadrant");
                }
            }

            public int LocationX
            {
                get { return (int)(Canvas.GetLeft(mRespawnIndicator)); }
                set
                {
                    Canvas.SetLeft(mRespawnIndicator, value);
                    mCheckpoint.Location.X = (float)value;
                    OnPropertyChanged("LocationX");
                }
            }

            public int LocationY
            {
                get { return (int)(Canvas.GetTop(mRespawnIndicator)); }
                set
                {
                    Canvas.SetTop(mRespawnIndicator, value);
                    mCheckpoint.Location.Y = (float)value;
                    OnPropertyChanged("LocationY");
                }
            }

            public string Text
            {
                get { return mCheckpoint.NarrationText; }
                set
                {
                    mCheckpoint.NarrationText = value;
                    OnPropertyChanged("Text");
                }
            }

            public CheckpointModel(Checkpoint checkpoint, DesignerItem item, Level level, Canvas canvas)
                : base(item, level)
            {
                if(!level.Checkpoints.Contains(checkpoint))
                    level.Checkpoints.Add(checkpoint);

                mCheckpoint = checkpoint;
                mCanvas = canvas;

                mCheckpointRegion = new System.Windows.Shapes.Rectangle();
                mCheckpointRegion.Fill = new SolidColorBrush(Colors.Yellow);
                mCheckpointRegion.Fill.Opacity = .25;
                mCheckpointRegion.IsHitTestVisible = false;

                Ellipse rindicator = new Ellipse();
                rindicator.Fill = Brushes.SpringGreen;
                rindicator.Stroke = Brushes.Black;
 
                TextBlock txt = new TextBlock();
                txt.Text = "R";
                txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
 
                Grid gr = new Grid();
                gr.Children.Add(rindicator);
                gr.Children.Add(txt);
                gr.IsHitTestVisible = false;

                mRespawnIndicator = new DesignerItem();
                mRespawnIndicator.CanResize = false;
                mRespawnIndicator.CanDelete = false;
                mRespawnIndicator.Model = this;
                mRespawnIndicator.Height = 30;
                mRespawnIndicator.Width = 30;
                mRespawnIndicator.MinWidth = 30;
                mRespawnIndicator.MinHeight = 30;
                mRespawnIndicator.Content = gr;

                canvas.Children.Add(mCheckpointRegion);
                canvas.Children.Add(mRespawnIndicator);

                X = (int)checkpoint.Bound.X;
                Y = (int)checkpoint.Bound.Y;
                LocationX = (int)checkpoint.Location.X;
                LocationY = (int)checkpoint.Location.Y;
                Quadrant = Quadrant;

                Canvas.SetZIndex(mRespawnIndicator, 1);
                Canvas.SetZIndex(mCheckpointRegion, -9);
            }

            public override List<UIElement> RemoveFromLevel()
            {
                var list = base.RemoveFromLevel();
                list.AddRange(Children);

                mLevel.Checkpoints.Remove(mCheckpoint);
                return list;
            }

            public override void Moved()
            {
                base.Moved();
                LocationX = LocationX;
                LocationY = LocationY;
            }
        }

        public CheckpointPropertiesWindow(Checkpoint checkpoint, Canvas canvas, DesignerItem item, Level level)
            : base()
        {
            InitializeComponent();
            item.PropertyWindow = this;
            DataContext = new CheckpointModel(checkpoint, item, level, canvas);
            (DataContext as CheckpointModel).RespawnIndicator.OnSelected += HandleSelection;
        }

        public override void HandleSelection(object sender, BoolEventArgs e)
        {
            base.HandleSelection(sender, e);
            foreach(UIElement ele in (DataContext as CheckpointModel).Children)
                ele.Visibility = e.Value ? Visibility.Visible : Visibility.Hidden;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using Half_Caked;

namespace LevelCreator
{
    public class DesignerCanvas : Canvas
    {
        private int mTabCount = 10;
        public Level Level;
                
        private Point? dragStartPoint = null;

        public IEnumerable<DesignerItem> SelectedItems
        {
            get
            {
                var selectedItems = from item in this.Children.OfType<DesignerItem>()
                                    where item.IsSelected == true
                                    select item;

                return selectedItems;
            }
        }

        public void DeselectAll()
        {
            foreach (DesignerItem item in this.SelectedItems)
            {
                item.IsSelected = false;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this)
            {
                this.dragStartPoint = new Point?(e.GetPosition(this));
                this.DeselectAll();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.dragStartPoint = null;
            }

            ScrollViewer parent = null;
            if(Parent is Border)
                parent = (Parent as Border).Parent as ScrollViewer;
            else
                parent = Parent as ScrollViewer;
            
            if (parent != null)
            {
                var point = e.GetPosition(parent);
                if (point.X < parent.ViewportWidth / 6)
                {
                    if (point.Y < parent.ViewportHeight / 6)
                    {
                        parent.ScrollToHorizontalOffset(Math.Max(parent.HorizontalOffset - 40 / (1 + point.X * 300 / parent.ViewportWidth), 0));
                        parent.ScrollToVerticalOffset(Math.Max(parent.VerticalOffset - 40 / (1 + point.Y * 300 / parent.ViewportHeight), 0));
                    }
                    else if (point.Y > parent.ViewportHeight * 5.0 / 6 && point.Y < parent.ViewportHeight)
                    {
                        parent.ScrollToHorizontalOffset(Math.Max(parent.HorizontalOffset - 40 / (1 + point.X * 300 / parent.ViewportWidth), 0));
                        parent.ScrollToVerticalOffset(Math.Min(parent.VerticalOffset + 40 / (1 + 300 * (parent.ViewportHeight - point.Y) / parent.ViewportHeight), parent.ScrollableHeight));
                    }
                }
                else if (point.X > parent.ViewportWidth * 5.0 / 6 && point.X < parent.ViewportWidth)
                {
                    if (point.Y < parent.ViewportHeight / 6)
                    {
                        parent.ScrollToHorizontalOffset(Math.Min(parent.HorizontalOffset + 40 / (1 + 300 * (parent.ViewportWidth - point.X) / parent.ViewportWidth), parent.ScrollableWidth));
                        parent.ScrollToVerticalOffset(Math.Max(parent.VerticalOffset - 40 / (1 + point.Y * 300 / parent.ViewportHeight), 0));
                    }
                    else if (point.Y > parent.ViewportHeight * 5.0 / 6 && point.Y < parent.ViewportHeight)
                    {
                        parent.ScrollToHorizontalOffset(Math.Min(parent.HorizontalOffset + 40 / (1 + 300 * (parent.ViewportWidth - point.X) / parent.ViewportWidth), parent.ScrollableWidth));
                        parent.ScrollToVerticalOffset(Math.Min(parent.VerticalOffset + 40 / (1 + 300 * (parent.ViewportHeight - point.Y) / parent.ViewportHeight), parent.ScrollableHeight));
                    }
                }
            }                

            if (this.dragStartPoint.HasValue)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(this, this.dragStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }

                e.Handled = true;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            string xamlString = e.Data.GetData("DESIGNER_ITEM") as string;
            if (!String.IsNullOrEmpty(xamlString))
            {
                DesignerItem newItem = null;
                FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(xamlString))) as FrameworkElement;

                if (content != null)
                {
                    newItem = new DesignerItem();
                    newItem.Content = content;
                    newItem.TabIndex = mTabCount++;
                    
                    Point position = e.GetPosition(this);

                    Image image = content as Image;
                    
                    if (image != null && !image.ToolTip.ToString().Contains("Tile"))
                    {
                        newItem.CanResize = false;
                        newItem.Height = newItem.MinHeight = newItem.MaxHeight = image.Source.Height;
                        newItem.Width = newItem.MinWidth = newItem.MaxWidth = image.Source.Width;
                    }
                    else
                    {
                        newItem.Width = 50;
                        newItem.Height = 50;
                        newItem.MinHeight = 2;
                        newItem.MinWidth = 2;
                    }
                    
                    switch (image.ToolTip.ToString())
                    {
                        case "Tile":
                            newItem.OnSelected += NewTileHandler(newItem);
                            break;
                        case "Door":
                            newItem.OnSelected += NewDoorHandler(newItem);
                            break;
                        case "Enemy":
                            newItem.OnSelected += NewEnemyHandler(newItem);
                            break;
                        case "Checkpoint":
                            newItem.OnSelected += NewCheckpointHandler(newItem);
                            break;
                        case "Switch":
                            newItem.OnSelected += NewSwitchHandler(newItem);
                            break;
                        case "Platform":
                            newItem.OnSelected += NewPlatformHandler(newItem);
                            break;
                        default:
                            break;
                    }

                    DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                    DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                    this.Children.Add(newItem);

                    if (newItem.PropertyWindow != null)
                        newItem.PropertyWindow.Moved();
                    else if (newItem.Model != null)
                        newItem.Model.Moved();


                    this.DeselectAll();
                    newItem.IsSelected = true;
                }

                e.Handled = true;
            }
        }

        EventHandler<BoolEventArgs> NewTileHandler(DesignerItem item)
        {
            Rectangle rect=  new Rectangle();
            rect.IsHitTestVisible = false;
            rect.Fill = (Window.GetWindow(this) as MainWindow).Settings.NormalBrush;
            rect.MinHeight = 2;
            rect.MinWidth = 2;

            item.Content = rect;

            Tile t = new Tile();
            t.Type = Surface.Normal;
            t.Dimensions.Width = 50;
            t.Dimensions.Height = 50;

            TilePropertiesWindow tpw = new TilePropertiesWindow(t, item, Level);
            this.Children.Add(tpw);
            return tpw.SelectionHandler;
        }

        EventHandler<BoolEventArgs> NewDoorHandler(DesignerItem item)
        {
            DoorModel dm = new DoorModel(new Door(), item, Level);
            DoorPropertiesWindow dpw = new DoorPropertiesWindow();
            dpw.DataContext = dm;
            item.PropertyWindow = dpw;

            this.Children.Add(dpw);
            return dpw.SelectionHandler;
        }

        EventHandler<BoolEventArgs> NewCheckpointHandler(DesignerItem item)
        {
            Checkpoint cp = new Checkpoint();
            CheckpointPropertiesWindow cpw = new CheckpointPropertiesWindow(cp, this, item, Level);
            this.Children.Add(cpw);
            return cpw.SelectionHandler;
        }

        EventHandler<BoolEventArgs> NewPlatformHandler(DesignerItem item)
        {
            Platform en = new Platform();
            PlatformPropertiesWindow epw = new PlatformPropertiesWindow(en, this, item, Level);
            this.Children.Add(epw);
            return epw.SelectionHandler;
        }

        EventHandler<BoolEventArgs> NewSwitchHandler(DesignerItem item)
        {
            Switch sw = new Switch();
            SwitchPropertiesWindow spw = new SwitchPropertiesWindow(sw, item, Level);
            this.Children.Add(spw);
            return spw.SelectionHandler;
        }

        EventHandler<BoolEventArgs> NewEnemyHandler(DesignerItem item)
        {
            Enemy en = new Enemy();
            EnemyPropertiesWindow epw = new EnemyPropertiesWindow(en, this, item, Level);
            this.Children.Add(epw);
            return epw.SelectionHandler;
        }
        /*
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();
            foreach (UIElement element in Children)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }

            // add some extra margin
            size.Width += 10;
            size.Height += 10;
            return size;
        }*/
    }
}

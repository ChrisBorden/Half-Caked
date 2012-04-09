using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LevelCreator
{
    public class MoveThumb : Thumb
    {
        private DesignerItem designerItem;
        private DesignerCanvas designerCanvas;

        public MoveThumb()
        {
            DragStarted += new DragStartedEventHandler(this.MoveThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.designerItem = DataContext as DesignerItem;

            if (this.designerItem != null)
            {
                this.designerCanvas = VisualTreeHelper.GetParent(this.designerItem) as DesignerCanvas;
            }
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.designerItem != null && this.designerCanvas != null && this.designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                {
                    minLeft = Math.Min(Canvas.GetLeft(item), minLeft);
                    minTop = Math.Min(Canvas.GetTop(item), minTop);
                }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                {
                    Canvas.SetLeft(item, Math.Min(Canvas.GetLeft(item) + deltaHorizontal, designerCanvas.ActualWidth - item.Width));
                    Canvas.SetTop(item, Math.Min(Canvas.GetTop(item) + deltaVertical, designerCanvas.ActualHeight - item.Height));

                    if (item.PropertyWindow != null)
                        item.PropertyWindow.Moved();
                    else if (item.Model != null)
                        item.Model.Moved();
                }

                this.designerCanvas.InvalidateMeasure();
                e.Handled = true;
            }
        }
    }
}

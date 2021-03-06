﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;

namespace LevelCreator
{
    public class ZoomBox : Control
    {
        private Thumb zoomThumb;
        private Canvas zoomCanvas;
        private Slider zoomSlider;
        private ScaleTransform scaleTransform;
        private DesignerCanvas designerCanvas;

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ZoomBox));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            if (this.ScrollViewer == null)
                return;

            if(this.ScrollViewer.Content is Border)
                this.designerCanvas = (this.ScrollViewer.Content as Border).Child as DesignerCanvas;
            else
                this.designerCanvas = this.ScrollViewer.Content as DesignerCanvas;

            if (this.designerCanvas == null)
                throw new Exception("DesignerCanvas must not be null!");
            
            this.zoomThumb = Template.FindName("PART_ZoomThumb", this) as Thumb;
            if (this.zoomThumb == null)
                throw new Exception("PART_ZoomThumb template is missing!");

            this.zoomCanvas = Template.FindName("PART_ZoomCanvas", this) as Canvas;
            if (this.zoomCanvas == null)
                throw new Exception("PART_ZoomCanvas template is missing!");

            this.zoomSlider = Template.FindName("PART_ZoomSlider", this) as Slider;
            if (this.zoomSlider == null)
                throw new Exception("PART_ZoomSlider template is missing!");

            this.designerCanvas.LayoutUpdated += new EventHandler(this.DesignerCanvas_LayoutUpdated);

            this.zoomThumb.DragDelta += new DragDeltaEventHandler(this.Thumb_DragDelta);
            this.zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.ZoomSlider_ValueChanged);

            this.scaleTransform = new ScaleTransform();
            this.designerCanvas.LayoutTransform = this.scaleTransform;
        }

        #region Zoom Commands

        public void ZoomTo(double value)
        {
            this.zoomSlider.Value = value;
        }

        public void ZoomIn()
        {
            this.zoomSlider.Value = this.zoomSlider.Ticks[this.zoomSlider.Ticks.IndexOf(this.zoomSlider.Value) + 1];
        }

        public bool CanZoomIn()
        {
            return (this.zoomSlider.Value < 500);
        }

        public void ZoomOut()
        {
            this.zoomSlider.Value = this.zoomSlider.Ticks[this.zoomSlider.Ticks.IndexOf(this.zoomSlider.Value) - 1];
        }

        public bool CanZoomOut()
        {
            return (this.zoomSlider.Value > 25);
        }

        #endregion

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double scale = e.NewValue / e.OldValue;

            double halfViewportHeight = this.ScrollViewer.ViewportHeight / 2;
            double newVerticalOffset = ((this.ScrollViewer.VerticalOffset + halfViewportHeight) * scale - halfViewportHeight);

            double halfViewportWidth = this.ScrollViewer.ViewportWidth / 2;
            double newHorizontalOffset = ((this.ScrollViewer.HorizontalOffset + halfViewportWidth) * scale - halfViewportWidth);

            this.scaleTransform.ScaleX *= scale;
            this.scaleTransform.ScaleY *= scale;

            this.ScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            this.ScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);

            this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset + e.HorizontalChange / scale);
            this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset + e.VerticalChange / scale);
        }

        private void DesignerCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            
            Point point = this.designerCanvas.TranslatePoint(new Point(0,0), this.ScrollViewer);
            this.zoomThumb.Width = Math.Min(this.ScrollViewer.ViewportWidth * scale, this.zoomCanvas.ActualWidth - xOffset*2);
            this.zoomThumb.Height = Math.Min(this.ScrollViewer.ViewportHeight * scale, this.zoomCanvas.ActualHeight - yOffset*2);

            Canvas.SetLeft(this.zoomThumb, xOffset + this.ScrollViewer.HorizontalOffset * scale);
            Canvas.SetTop(this.zoomThumb, yOffset + this.ScrollViewer.VerticalOffset * scale);
        }
        
        private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
        {
            // designer canvas size
            double w = this.designerCanvas.ActualWidth * this.scaleTransform.ScaleX;
            double h = this.designerCanvas.ActualHeight * this.scaleTransform.ScaleY;

            // zoom canvas size
            double x = this.zoomCanvas.ActualWidth;
            double y = this.zoomCanvas.ActualHeight;

            double scaleX = x / w;
            double scaleY = y / h;

            scale = (scaleX < scaleY) ? scaleX : scaleY;

            xOffset = (x - scale * w) / 2;
            yOffset = (y - scale * h) / 2;
        }
    }
}

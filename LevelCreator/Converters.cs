using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using System.Windows;

namespace LevelCreator
{
    public class StringToIntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return Int32.Parse(value.ToString());
            }
            catch { return -1; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
   
    public class IntToVector2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int quadrant = Int32.Parse(value.ToString());
                return new Vector2(quadrant % 3 == 1 ? -1 : 1, quadrant > 2 ? -1 : 1);
            }
            catch { return new Vector2(1,1); }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Vector2 quadrant = (Vector2)value;
                if (quadrant.X > 0)
                    if (quadrant.Y > 0)
                        return 3;
                    else
                        return 0;
                else
                    if (quadrant.Y < 0)
                        return 1;
                    else
                        return 2;
            }
            catch { return 3; }
        }
    }

    public class MouseWheelGesture : MouseGesture
    {
        public WheelDirection Direction { get; set; }

        public static MouseWheelGesture Up
        {
            get
            {
                return new MouseWheelGesture { Direction = WheelDirection.Up };
            }
        }

        public static MouseWheelGesture Down
        {
            get
            {
                return new MouseWheelGesture { Direction = WheelDirection.Down };
            }
        }

        public static MouseWheelGesture CtrlUp
        {
            get
            {
                return new MouseWheelGesture(ModifierKeys.Control) { Direction = WheelDirection.Up };
            }
        }

        public static MouseWheelGesture CtrlDown
        {
            get
            {
                return new MouseWheelGesture(ModifierKeys.Control) { Direction = WheelDirection.Down };
            }
        }


        public MouseWheelGesture()
            : base(MouseAction.WheelClick)
        {
        }

        public MouseWheelGesture(ModifierKeys modifiers)
            : base(MouseAction.WheelClick, modifiers)
        {
        }
        
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs)) return false;
            if (!(inputEventArgs is MouseWheelEventArgs)) return false;
            var args = (MouseWheelEventArgs)inputEventArgs;
            switch (Direction)
            {
                case WheelDirection.None:
                    return args.Delta == 0;
                case WheelDirection.Up:
                    return args.Delta > 0;
                case WheelDirection.Down:
                    return args.Delta < 0;
                default:
                    return false;
            }
        }

        public enum WheelDirection
        {
            None,
            Up,
            Down,
        }

    }
}

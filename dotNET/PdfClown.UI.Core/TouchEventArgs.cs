using SkiaSharp;
using System;
using System.ComponentModel;

namespace PdfClown.UI
{
    public class TouchEventArgs : HandledEventArgs
    {
        public TouchEventArgs(TouchAction action, MouseButton button)
        {
            ActionType = action;
            MouseButton = button;
        }

        public TouchAction ActionType { get; }
        public MouseButton MouseButton { get; }
        public int WheelDelta { get; set; }
        public SKPoint Location { get; set; }
        public long PointerId { get; set; }
    }
}

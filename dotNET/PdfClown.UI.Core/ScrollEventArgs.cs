using System.ComponentModel;

namespace PdfClown.UI
{
    public class ScrollEventArgs : HandledEventArgs
    {
        public ScrollEventArgs(int delta) : base(false)
        {
            WheelDelta = delta;
        }

        public int WheelDelta { get; }
    }
}

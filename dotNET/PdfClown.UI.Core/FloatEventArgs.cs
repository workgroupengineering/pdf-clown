using System;

namespace PdfClown.UI
{
    public delegate void FloatEventHandler(FloatEventArgs e);

    public class FloatEventArgs : EventArgs
    {
        public FloatEventArgs(float value)
        {
            Value = value;
        }

        public float Value { get; set; }
    }
}
using SkiaSharp;
using System;

namespace PdfClown.UI
{
    public interface IBoundHandler
    {
        SKRect Bounds { get; }
        event EventHandler BoundsChanged;
    }
}
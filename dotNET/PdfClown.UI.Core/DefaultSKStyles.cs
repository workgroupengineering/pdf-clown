using SkiaSharp;
using System.Threading;

namespace PdfClown.UI
{
    public static class DefaultSKStyles
    {
        public static readonly SKPaint PaintPageBackground = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.White };
        public static readonly SKPaint PaintRed = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.OrangeRed };
        public static readonly SKPaint PaintPointFill = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill };
        public static readonly SKPaint PaintSelectionRect = new SKPaint { Color = SKColors.LightGreen.WithAlpha(125), Style = SKPaintStyle.Fill };
        public static readonly SKPaint PaintTextSelectionFill = new SKPaint { Color = SKColors.LightBlue, Style = SKPaintStyle.Fill, BlendMode = SKBlendMode.Multiply, IsAntialias = true };
        public static readonly SKPaint PaintBorderDefault = new SKPaint { Color = SKColors.Silver, Style = SKPaintStyle.Stroke, StrokeWidth = 0, IsAntialias = true };
        public static readonly SKPaint PaintBorderSelection = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Stroke, StrokeWidth = 0, IsAntialias = true };
    }

    public static class Envir
    {
        public static void Init()
        {
            if (MainContext == null)
            {
                MainContext = SynchronizationContext.Current;
            }
        }

        public static SynchronizationContext MainContext { get; set; }
    }

}


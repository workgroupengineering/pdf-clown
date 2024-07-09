using SkiaSharp;

namespace PdfClown.UI.ToolTip
{
    public abstract class AnnotationToolTipRenderer : ToolTipRenderer
    {
        public override SKRect GetWindowBound(PdfViewState state)
        {
            SKRect windowArea = state.WindowArea;
            SKRect annotation = state.InvertWindowScaleMatrix.MapRect(state.AnnotationBound);
            SKRect toolTip = Measure();
            var left = annotation.MidX - toolTip.Width / 2;
            var right = left + toolTip.Width;
            return SKRect.Create(
                   left < windowArea.Left ? windowArea.Left : right > windowArea.Right ? left - (right - windowArea.Right) : left,
                   (annotation.Bottom + toolTip.Bottom + 10) > windowArea.Bottom ? (annotation.Top - toolTip.Height) : annotation.Bottom + 10,
                    toolTip.Width, toolTip.Height);
        }
    }

}

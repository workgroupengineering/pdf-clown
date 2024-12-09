namespace PdfClown.UI
{
    public interface ISKScrollView
    {
        double VValue { get; set; }
        double HValue { get; set; }
        double VMaximum { get; set; }
        double HMaximum { get; set; }
        bool IsVScrollAnimation { get; }
        bool IsHScrollAnimation { get; }

        void InvalidatePaint();
        void CapturePointer(long pointerId);
    }
}
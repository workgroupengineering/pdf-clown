using PdfClown.Util.Math.Geom;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PdfClown.UI.ToolTip
{
    public abstract class ToolTipRenderer
    {
        public const int Indent = 3;
        public static readonly float MaxWidth = 360;
        private static readonly char[] rnSplitters = new char[] { '\r', '\n' };

        private Dictionary<SKPaint, SKFont> fontCache = new Dictionary<SKPaint, SKFont>();
        protected List<LineOfText> ContentLines;
        protected SKRect ContentBound;
        protected readonly SKPaint paintToolTipText = new SKPaint { Style = SKPaintStyle.StrokeAndFill, Color = SKColors.Black, TextSize = 14, IsAntialias = true };
        protected readonly SKPaint paintToolTipHeadText = new SKPaint { Style = SKPaintStyle.StrokeAndFill, Color = SKColors.Black, TextSize = 12, IsAntialias = true, FakeBoldText = true };
        protected readonly SKPaint paintToolTipBackground = new SKPaint() { Color = SKColors.LightGoldenrodYellow, Style = SKPaintStyle.Fill, IsAntialias = true };
        protected readonly SKPaint paintToolTipBorder = new SKPaint() { Color = SKColors.DarkGray, Style = SKPaintStyle.Stroke, IsAntialias = true };

        protected ToolTipRenderer()
        {
        }

        public SKFont GetFont(SKPaint paint) => fontCache.TryGetValue(paint, out var font) ? font : (font = fontCache[paint] = paint.ToFont());

        protected void ClearLines()
        {
            if (ContentLines != null)
            {
                ContentLines.ForEach(x => x.Dispose());
                ContentLines.Clear();
            }
        }

        public List<LineOfText> MeasureText(string text, SKPaint paint, ref SKRect totalBounds)
        {
            var result = new List<LineOfText>();
            if (string.IsNullOrEmpty(text))
                return result;

            var enumerator = new StringSegmentEnumerator(text, paint, MaxWidth);
            while (enumerator.MoveNext())
            {
                var lof = MeasureLine(enumerator.Current, paint);

                lof.Bound.Offset(0, totalBounds.Bottom);

                totalBounds.Add(lof.Bound);

                result.Add(lof);
            }
            return result;
        }

        public LineOfText MeasureLine(ReadOnlySpan<char> text, SKPaint paint)
        {
            var textBlob = text.Length > 0 ? SKTextBlob.Create(text, GetFont(paint)) : null;
            var contentBound = textBlob?.Bounds ?? new SKRect(0, 0, 0, 10);

            return new LineOfText
            {
                Start = new SKPoint(-contentBound.Left, -contentBound.Top),
                Bound = SKRect.Create(contentBound.Width, contentBound.Height),
                Text = textBlob,
                Paint = paint
            };
        }

        public abstract SKRect Measure();

        public void Draw(PdfViewState state)
        {
            if (ContentLines == null)
                return;

            state.Canvas.Save();
            state.Canvas.SetMatrix(state.WindowScaleMatrix);

            var mapped = state.ToolTipBounds;
            state.Canvas.Translate(mapped.Left, mapped.Top);

            DrawContent(state.Canvas);

            state.Canvas.Restore();
        }

        public virtual void DrawContent(SKCanvas canvas)
        {
            canvas.DrawRect(ContentBound, paintToolTipBackground);
            canvas.DrawRect(ContentBound, paintToolTipBorder);
            canvas.Translate(Indent, Indent);

            foreach (var line in ContentLines)
            {
                line.Draw(canvas);
            }
        }

        public abstract SKRect GetWindowBound(PdfViewState state);

        public class LineOfText : IDisposable
        {
            public SKRect Bound;
            public SKTextBlob Text { get; set; }
            public SKPoint Start { get; set; }
            public SKPaint Paint { get; set; }

            public void Dispose()
            {
                Text?.Dispose();
                Text = null;
                Paint = null;
            }

            public void Draw(SKCanvas canvas)
            {
                if (Text == null)
                    return;
                canvas.DrawText(Text, Start.X + Bound.Left, Start.Y + Bound.Top, Paint);
            }
        }

        private struct StringSegmentEnumerator
        {
            private readonly string _s;
            private readonly SKPaint _paint;
            private int _start;
            private int _length;
            private float _maxWidth;

            public StringSegmentEnumerator(string s, SKPaint paint, float maxWidth)
            {
                _s = s;
                _paint = paint;
                _start = 0;
                _length = 0;
                _maxWidth = maxWidth;
            }

            public readonly ReadOnlySpan<char> Current => _s.AsSpan(_start, _length);

            public bool MoveNext()
            {
                var currentPosition = _start + _length;

                if (currentPosition >= _s.Length)
                    return false;

                int start = -1;
                bool isBreak;
                char c;
                while (currentPosition < _s.Length)
                {
                    c = _s[currentPosition];
                    isBreak = IsBreak(c);
                    if (isBreak && start > -1)
                        break;
                    if (!isBreak && start == -1 && !IsTrimm(c))
                        start = currentPosition;
                    currentPosition++;
                }
                if (start == -1)
                    return false;

                if ((currentPosition - start) > 0 && float.IsFinite(_maxWidth))
                {
                    var span = _s.AsSpan(start, currentPosition - start);
                    var breakLength = (int)_paint.BreakText(span, _maxWidth);
                    currentPosition -= span.Length - breakLength;
                }

                _start = start;
                _length = currentPosition - start;

                return true;
            }

            private readonly bool IsBreak(char c)
            {
                return c == '\r'
                    || c == '\n';
            }

            private readonly bool IsTrimm(char c)
            {
                return c == ' ';
            }
        }
    }

}

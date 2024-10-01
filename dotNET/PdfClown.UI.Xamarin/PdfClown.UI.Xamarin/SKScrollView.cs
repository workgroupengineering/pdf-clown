using PdfClown.Tools;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;

namespace PdfClown.UI
{
    public class SKScrollView : SKCanvasView
    {
        public static readonly BindableProperty CursorProperty = BindableProperty.Create(nameof(Cursor), typeof(CursorType), typeof(SKScrollView), CursorType.Arrow,
            propertyChanged: (bindable, oldValue, newValue) => ((SKScrollView)bindable).OnCursorChanged((CursorType)oldValue, (CursorType)newValue));

        public static readonly BindableProperty VerticalMaximumProperty = BindableProperty.Create(nameof(VerticalMaximum), typeof(double), typeof(SKScrollView), 1D,
            propertyChanged: (bidable, oldValue, newValue) => ((SKScrollView)bidable).OnVerticalMaximumChanged((double)oldValue, (double)newValue));

        public static readonly BindableProperty HorizontalMaximumProperty = BindableProperty.Create(nameof(HorizontalMaximum), typeof(double), typeof(SKScrollView), 1D,
            propertyChanged: (bidable, oldValue, newValue) => ((SKScrollView)bidable).OnHorizontalMaximumChanged((double)oldValue, (double)newValue));

        public static readonly BindableProperty VerticalValueProperty = BindableProperty.Create(nameof(VerticalValue), typeof(double), typeof(SKScrollView), 0D,
            propertyChanged: (bidable, oldValue, newValue) => ((SKScrollView)bidable).OnVerticalValueChanged((double)oldValue, (double)newValue));

        public static readonly BindableProperty HorizontalValueProperty = BindableProperty.Create(nameof(HorizontalValue), typeof(double), typeof(SKScrollView), 0D,
            propertyChanged: (bidable, oldValue, newValue) => ((SKScrollView)bidable).OnHorizontalValueChanged((double)oldValue, (double)newValue));

        protected EventHandler<ScrollEventArgs> verticalScrolledHandler;
        protected EventHandler<ScrollEventArgs> нorizontalScrolledHandler;


        private static readonly SKPaint bottonPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = 1,
            IsAntialias = true,
            Color = new SKColor(190, 190, 190)
        };

        private static readonly SKPaint bottonPaintHover = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = 1,
            IsAntialias = true,
            Color = new SKColor(160, 160, 160)
        };

        private static readonly SKPaint bottonPaintPressed = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = 1,
            IsAntialias = true,
            Color = new SKColor(120, 120, 120)
        };

        private static readonly SKPaint scrollPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = 1,
            IsAntialias = true,
            Color = new SKColor(240, 240, 240)
        };

        private static readonly SKPaint svgPaint = new SKPaint
        {
            IsAntialias = true,
            ColorFilter = SKColorFilter.CreateBlendMode(SKColors.Black, SKBlendMode.SrcIn),
            FilterQuality = SKFilterQuality.Medium
        };

        private static readonly SvgImage upSvg = SvgImage.GetCache(typeof(Orientation).Assembly, "caret-up");
        private static readonly SvgImage downSvg = SvgImage.GetCache(typeof(Orientation).Assembly, "caret-down");
        private static readonly SvgImage leftSvg = SvgImage.GetCache(typeof(Orientation).Assembly, "caret-left");
        private static readonly SvgImage rightSvg = SvgImage.GetCache(typeof(Orientation).Assembly, "caret-right");
        private static readonly SvgImage shiftVSvg = SvgImage.GetCache(typeof(Orientation).Assembly, "line");
        private static readonly SvgImage shiftHSvg = SvgImage.GetCache(typeof(Orientation).Assembly, "link");


        private const string ahScroll = "ScrollAnimation";
        private const string ahVerticalScroll = "VerticalScrollAnimation";
        private const string ahHorizontalScroll = "HorizontalScrollAnimation";
        private SKPoint nullLocation;

        private Orientation nullDirection = Orientation.Vertical;
        private double vHeight;
        private double vWidth;
        private double vsHeight;
        private double hsWidth;
        private double kWidth;
        private double kHeight;
        private bool verticalHovered;
        private bool нorizontalHovered;
        private SKRect verticalPadding = new SKRect(0, 0, 0, DefaultSKStyles.StepSize);
        private SKRect horizontalPadding = new SKRect(0, 0, DefaultSKStyles.StepSize, 0);
        private bool isDrawing;
        private SvgImage hoverButton;
        private SvgImage pressedButton;

        public SKScrollView()
        {
            IsTabStop = true;
            IgnorePixelScaling = false;
            EnableTouchEvents = true;
            Touch += OnTouch;
        }

        public CursorType Cursor
        {
            get => (CursorType)GetValue(CursorProperty);
            set => SetValue(CursorProperty, value);
        }

        public bool AllowScrollView { get; set; } = true;

        public event EventHandler<ScrollEventArgs> VerticalScrolled
        {
            add => verticalScrolledHandler += value;
            remove => verticalScrolledHandler -= value;
        }

        public event EventHandler<ScrollEventArgs> HorizontalScrolled
        {
            add => нorizontalScrolledHandler += value;
            remove => нorizontalScrolledHandler -= value;
        }
        public event EventHandler<CanvasKeyEventArgs> KeyDown;

        public double VerticalMaximum
        {
            get => (double)GetValue(VerticalMaximumProperty);
            set => SetValue(VerticalMaximumProperty, value);
        }

        public double HorizontalMaximum
        {
            get => (double)GetValue(HorizontalMaximumProperty);
            set => SetValue(HorizontalMaximumProperty, value);
        }

        public double VerticalValue
        {
            get => (double)GetValue(VerticalValueProperty);
            set
            {
                if (!VerticalScrollBarVisible && value != 0)
                {
                    SetValue(VerticalValueProperty, 0);
                }
                else
                {
                    var max = VerticalMaximum - Height + DefaultSKStyles.StepSize;
                    value = value < 0 || max < 0 ? 0 : value > max ? max : value;

                    SetValue(VerticalValueProperty, value);
                }
            }
        }

        public double HorizontalValue
        {
            get => (double)GetValue(HorizontalValueProperty);
            set
            {
                var max = HorizontalMaximum - Width + DefaultSKStyles.StepSize;
                value = value < 0 || max < 0 ? 0 : value > max ? max : value;

                SetValue(HorizontalValueProperty, value);
            }
        }

        public KeyModifiers KeyModifiers { get; set; }

        public Animation ScrollAnimation { get; private set; }
        public Animation VerticalScrollAnimation { get; private set; }
        public Animation HorizontalScrollAnimation { get; private set; }

        public double VericalSize => VerticalHovered ? DefaultSKStyles.MaxSize : DefaultSKStyles.MinSize;

        public double HorizontalSize => HorizontalHovered ? DefaultSKStyles.MaxSize : DefaultSKStyles.MinSize;

        public SKRect VerticalPadding
        {
            get => verticalPadding;
            set
            {
                if (verticalPadding != value)
                {
                    verticalPadding = value;
                    OnVerticalMaximumChanged(1, VerticalMaximum);
                }
            }
        }

        public SKRect HorizontalPadding
        {
            get => horizontalPadding;
            set
            {
                if (horizontalPadding != value)
                {
                    horizontalPadding = value;
                    OnHorizontalMaximumChanged(1, HorizontalMaximum);
                }
            }
        }

        public bool VerticalHovered
        {
            get => verticalHovered;
            set
            {
                if (verticalHovered != value)
                {
                    verticalHovered = value;
                    OnPropertyChanged();
                    OnVerticalMaximumChanged(1, VerticalMaximum);
                    InvalidateSurface();
                }
            }
        }

        public bool HorizontalHovered
        {
            get => нorizontalHovered;
            set
            {
                if (нorizontalHovered != value)
                {
                    нorizontalHovered = value;
                    OnPropertyChanged();
                    OnHorizontalMaximumChanged(1, HorizontalMaximum);
                    InvalidateSurface();
                }
            }
        }

        protected SvgImage PressedButton
        {
            get => pressedButton;
            set
            {
                if (pressedButton != value)
                {
                    pressedButton = value;
                    InvalidateSurface();
                }
            }
        }

        protected SvgImage HoverButton
        {
            get => hoverButton;
            set
            {
                if (hoverButton != value)
                {
                    hoverButton = value;
                    InvalidateSurface();
                }
            }
        }

        public event EventHandler<SKPaintSurfaceEventArgs> PaintContent;

        public event EventHandler<SKPaintSurfaceEventArgs> PaintOver;

        public bool VerticalScrollBarVisible => VerticalMaximum >= (Height + DefaultSKStyles.StepSize);

        public bool HorizontalScrollBarVisible => HorizontalMaximum >= (Width + DefaultSKStyles.StepSize);

        public bool WheelTouchSupported { get; set; } = true;

        public event EventHandler ScrollComplete;

        protected override void OnTouch(SKTouchEventArgs e)
        {
            base.OnTouch(e);
            if (WheelTouchSupported && e.ActionType == SKTouchAction.WheelChanged)
            {
                OnScrolled(e.WheelDelta);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            OnVerticalMaximumChanged(1, VerticalMaximum);
            OnHorizontalMaximumChanged(1, HorizontalMaximum);
        }

        private void OnVerticalMaximumChanged(double oldValue, double newValue)
        {
            vsHeight = (Height - (verticalPadding.Top + verticalPadding.Bottom)) - VericalSize * 2;
            kHeight = vsHeight / newValue;
            vHeight = (float)((Height - DefaultSKStyles.StepSize) * kHeight);

            if (vHeight < VericalSize)
            {
                vHeight = VericalSize;
                vsHeight = (Height - (verticalPadding.Top + verticalPadding.Bottom)) - (VericalSize * 2 + vHeight);
                kHeight = vsHeight / newValue;
            }
            //else
            //{
            //    vHeight += VericalSize;
            //}

            VerticalValue = VerticalValue;
        }

        private void OnHorizontalMaximumChanged(double oldValue, double newValue)
        {
            hsWidth = (Width - (horizontalPadding.Left + horizontalPadding.Right)) - HorizontalSize * 2;
            kWidth = hsWidth / newValue;
            vWidth = (float)((Width - DefaultSKStyles.StepSize) * kWidth);

            //if (vWidth < HorizontalSize)
            //{
            //    vWidth = HorizontalSize;
            //    hsWidth = (Width - horizontalPadding.HorizontalThickness) - (HorizontalSize * 2 + vWidth);
            //    kWidth = hsWidth / newValue;
            //}
            //else
            //{
            //    vWidth += HorizontalSize;
            //}

            HorizontalValue = HorizontalValue;
        }

        protected virtual void OnVerticalValueChanged(double oldValue, double newValue)
        {
            if (VerticalScrollAnimation == null)
            {
                verticalScrolledHandler?.Invoke(this, new ScrollEventArgs((int)newValue));
                InvalidateSurface();
            }
            else
            {
                InvalidateSurface();
            }
        }

        protected virtual void OnHorizontalValueChanged(double oldValue, double newValue)
        {
            if (HorizontalScrollAnimation == null)
            {
                InvalidateSurface();
                нorizontalScrolledHandler?.Invoke(this, new ScrollEventArgs((int)newValue));
            }
            else
            {
                InvalidateSurface();
            }
        }

        private void OnTargetChanged(SKScrollView oldValue, SKScrollView newValue)
        {
            if (oldValue != null)
            {
                oldValue.VerticalScrolled -= OnTargetScrolled;
            }

            if (newValue != null)
            {
                newValue.VerticalScrolled += OnTargetScrolled;
            }
        }

        protected virtual void OnTouch(object sender, SKTouchEventArgs e)
        {
            //if (ScrollBarVisible)
            //{
            //    var scrollBound = GetScrollBounds();
            //    if (scrollBound.Contains(e.Location))
            //    {
            //        e.Handled = true;
            //    }
            //}
            switch (e.ActionType)
            {
                case SKTouchAction.Released:
                    PressedButton = null;
                    if (nullLocation != SKPoint.Empty)
                    {
                        nullLocation = SKPoint.Empty;
                        e.Handled = true;
                    }
                    goto case SKTouchAction.Exited;
                case SKTouchAction.Exited:
                    HoverButton = null;
                    goto case SKTouchAction.Moved;
                case SKTouchAction.Moved:
                    if (nullLocation == SKPoint.Empty)
                    {
                        VerticalHovered = VerticalScrollBarVisible && GetVerticalScrollBounds().Contains(e.Location);
                        HorizontalHovered = HorizontalScrollBarVisible && GetHorizontalScrollBounds().Contains(e.Location);

                        if (VerticalHovered)
                        {
                            e.Handled = true;

                            if (GetUpBounds().Contains(e.Location))
                            {
                                HoverButton = upSvg;
                            }
                            else if (GetDownBounds().Contains(e.Location))
                            {
                                HoverButton = downSvg;
                            }
                            else if (GetVerticalValueBounds().Contains(e.Location))
                            {
                                HoverButton = shiftVSvg;
                            }
                            else
                            {
                                HoverButton = null;
                            }
                        }
                        else if (HorizontalHovered)
                        {
                            e.Handled = true;

                            if (GetLeftBounds().Contains(e.Location))
                            {
                                HoverButton = leftSvg;
                            }
                            else if (GetRightBounds().Contains(e.Location))
                            {
                                HoverButton = rightSvg;
                            }
                            else if (GetHorizontalValueBounds().Contains(e.Location))
                            {
                                HoverButton = shiftHSvg;
                            }
                            else
                            {
                                HoverButton = null;
                            }
                        }
                        else
                            HoverButton = null;
                    }
                    break;
                case SKTouchAction.WheelChanged:
                    //if (VerticalScrollBarVisible)
                    //{
                    //    var temp = VerticalValue;
                    //    VerticalValue = VerticalValue - step * Math.Sign(e.WheelDelta);
                    //    if (temp != VerticalValue)
                    //    {
                    //        e.Handled = true;
                    //    }
                    //}
                    break;
            }

            if ((!VerticalScrollBarVisible && !HorizontalScrollBarVisible)
                || e.MouseButton != SKMouseButton.Left)
                return;
            switch (e.ActionType)
            {
                case SKTouchAction.Moved:
                    if (nullLocation != SKPoint.Empty)
                    {
                        var newLocation = e.Location - nullLocation;
                        if (nullDirection == Orientation.Vertical)
                        {
                            VerticalValue += newLocation.Y / kHeight;
                        }
                        else if (nullDirection == Orientation.Horizontal)
                        {
                            HorizontalValue += newLocation.X / kWidth;
                        }
                        nullLocation = e.Location;
                        e.Handled = true;
                    }
                    break;
                case SKTouchAction.Pressed:
                    var verticalScrollBound = GetVerticalScrollBounds();
                    var нorizontalScrollBound = GetHorizontalScrollBounds();
                    if (verticalScrollBound.Contains(e.Location))
                    {
                        e.Handled = true;
                        var upBound = GetUpBounds();
                        if (upBound.Contains(e.Location))
                        {
                            VerticalValue -= DefaultSKStyles.StepSize;
                            PressedButton = upSvg;
                            return;
                        }

                        var downBound = GetDownBounds();
                        if (downBound.Contains(e.Location))
                        {
                            VerticalValue += DefaultSKStyles.StepSize;
                            PressedButton = downSvg;
                            return;
                        }

                        var valueBound = GetVerticalValueBounds();
                        if (valueBound.Contains(e.Location))
                        {
                            nullLocation = e.Location;
                            nullDirection = Orientation.Vertical;
                            PressedButton = shiftVSvg;
                            return;
                        }

                        VerticalValue = e.Location.Y / kHeight - Height / 2;
                    }
                    if (нorizontalScrollBound.Contains(e.Location))
                    {
                        e.Handled = true;
                        var leftBound = GetLeftBounds();
                        if (leftBound.Contains(e.Location))
                        {
                            HorizontalValue -= DefaultSKStyles.StepSize;
                            PressedButton = leftSvg;
                            return;
                        }

                        var rightBound = GetRightBounds();
                        if (rightBound.Contains(e.Location))
                        {
                            HorizontalValue += DefaultSKStyles.StepSize;
                            PressedButton = rightSvg;
                            return;
                        }

                        var valueBound = GetHorizontalValueBounds();
                        if (valueBound.Contains(e.Location))
                        {
                            nullLocation = e.Location;
                            nullDirection = Orientation.Horizontal;
                            PressedButton = shiftHSvg;
                            return;
                        }

                        HorizontalValue = e.Location.X / kWidth - Width / 2;
                    }
                    break;
            }
        }

        private void OnTargetScrolled(object sender, ScrollEventArgs e)
        {
            OnScrolled(e.WheelDelta);
        }

        public virtual bool OnScrolled(int delta)
        {
            var temp = VerticalValue;
            VerticalValue = VerticalValue - delta;
            return temp != VerticalValue;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var XScaleFactor = (float)(e.Info.Width / Width);
            var YScaleFactor = (float)(e.Info.Height / Height);

            canvas.Scale(XScaleFactor, YScaleFactor);
            canvas.Clear(BackgroundColor.ToSKColor());
            base.OnPaintSurface(e);

            if (!IsVisible || isDrawing)
                return;
            isDrawing = true;
            try
            {
                OnPaintContent(e);
                PaintScrollBars(canvas);
                PaintOver?.Invoke(this, e);
            }
            finally
            {
                isDrawing = false;
            }
        }

        private void PaintScrollBars(SKCanvas canvas)
        {
            if (VerticalScrollBarVisible)
            {
                canvas.DrawRect(GetVerticalScrollBounds(), scrollPaint);
                // if (Hovered)
                {
                    var upBound = GetUpBounds();
                    if (HoverButton == upSvg)
                        canvas.DrawRect(upBound, PressedButton == upSvg ? bottonPaintPressed : bottonPaintHover);
                    SvgImage.DrawImage(canvas, upSvg, svgPaint, upBound, 0.5F);

                    var downBound = GetDownBounds();
                    if (HoverButton == downSvg)
                        canvas.DrawRect(downBound, PressedButton == downSvg ? bottonPaintPressed : bottonPaintHover);
                    SvgImage.DrawImage(canvas, downSvg, svgPaint, downBound, 0.5F);
                }
                var valueBound = GetVerticalValueBounds();
                valueBound.Inflate(-1, -1);
                canvas.DrawRect(valueBound, HoverButton == shiftVSvg ? PressedButton == shiftVSvg ? bottonPaintPressed : bottonPaintHover : bottonPaint);
            }

            if (HorizontalScrollBarVisible)
            {
                canvas.DrawRect(GetHorizontalScrollBounds(), scrollPaint);
                // if (Hovered)
                {
                    var leftBound = GetLeftBounds();
                    if (HoverButton == leftSvg)
                        canvas.DrawRect(leftBound, PressedButton == leftSvg ? bottonPaintPressed : bottonPaintHover);
                    SvgImage.DrawImage(canvas, leftSvg, svgPaint, leftBound, 0.5F);

                    var rightBound = GetRightBounds();
                    if (HoverButton == rightSvg)
                        canvas.DrawRect(rightBound, PressedButton == rightSvg ? bottonPaintPressed : bottonPaintHover);
                    SvgImage.DrawImage(canvas, rightSvg, svgPaint, rightBound, 0.5F);
                }
                var valueBound = GetHorizontalValueBounds();
                valueBound.Inflate(-1, -1);
                canvas.DrawRect(valueBound, HoverButton == shiftHSvg ? PressedButton == shiftHSvg ? bottonPaintPressed : bottonPaintHover : bottonPaint);
            }
        }

        protected virtual void OnPaintContent(SKPaintSurfaceEventArgs e)
        {
            PaintContent?.Invoke(this, e);
        }

        public SKRect GetVerticalScrollBounds()
        {
            return SKRect.Create((float)(Width - VericalSize),
                (float)verticalPadding.Top,
                (float)VericalSize,
                (float)(Height - (verticalPadding.Top + verticalPadding.Bottom)));
        }

        public SKRect GetHorizontalScrollBounds()
        {
            return SKRect.Create((float)horizontalPadding.Left,
                (float)(Height - HorizontalSize),
                (float)(Width - (horizontalPadding.Left + horizontalPadding.Right)),
                (float)HorizontalSize);
        }

        private SKRect GetUpBounds()
        {
            return SKRect.Create((float)(Width - VericalSize),
                (float)verticalPadding.Top,
                (float)VericalSize, (float)VericalSize);
        }

        private SKRect GetDownBounds()
        {
            return SKRect.Create((float)(Width - VericalSize),
                (float)(Height - (VericalSize + verticalPadding.Bottom)),
                (float)VericalSize, (float)VericalSize);
        }

        private SKRect GetLeftBounds()
        {
            return SKRect.Create((float)horizontalPadding.Left,
                (float)(Height - HorizontalSize),
                (float)HorizontalSize, (float)HorizontalSize);
        }

        private SKRect GetRightBounds()
        {
            return SKRect.Create((float)(Width - (HorizontalSize + horizontalPadding.Right)),
                (float)(Height - HorizontalSize),
                (float)HorizontalSize, (float)HorizontalSize);
        }

        private SKRect GetVerticalValueBounds()
        {
            var top = VericalSize + verticalPadding.Top + (float)(VerticalValue * kHeight);
            return SKRect.Create((float)(Width - VericalSize),
                (float)top, (float)VericalSize, (float)vHeight);
        }

        private SKRect GetHorizontalValueBounds()
        {
            var left = HorizontalSize + horizontalPadding.Left + (HorizontalValue * kWidth);
            return SKRect.Create((float)left, (float)(Height - HorizontalSize), (float)vWidth, (float)HorizontalSize);
        }

        public void SetVerticalScrolledPosition(double top)
        {
            VerticalValue = top;
        }

        public void HorizontalAnimateScroll(double newValue, uint rate = DefaultSKStyles.MaxSize, uint legth = 400, Easing asing = null)
        {
            if (HorizontalValue == newValue)
                return;
            if (HorizontalScrollAnimation != null)
            {
                this.AbortAnimation(ahHorizontalScroll);
            }
            HorizontalScrollAnimation = new Animation(v => HorizontalValue = v, HorizontalValue, newValue, asing);
            HorizontalScrollAnimation.Commit(this, ahHorizontalScroll, rate, legth, finished: OnScrollComplete);
        }

        public void VerticalAnimateScroll(double newValue, uint rate = DefaultSKStyles.MaxSize, uint legth = 400, Easing asing = null)
        {
            if (VerticalValue == newValue)
                return;
            if (VerticalScrollAnimation != null)
            {
                this.AbortAnimation(ahVerticalScroll);
            }
            VerticalScrollAnimation = new Animation(v => VerticalValue = v, VerticalValue, newValue, asing);
            VerticalScrollAnimation.Commit(this, ahVerticalScroll, rate, legth, finished: OnScrollComplete);
        }

        protected void AnimateScroll(float top, double left)
        {
            if (VerticalValue == top
                && HorizontalValue == left)
                return;
            if (ScrollAnimation != null)
            {
                this.AbortAnimation(ahScroll);
            }
            ScrollAnimation = new Animation();
            ScrollAnimation.Add(0, 1, new Animation(v => VerticalValue = v, VerticalValue, top, Easing.SinOut));
            ScrollAnimation.Add(0, 1, new Animation(v => HorizontalValue = v, HorizontalValue, left, Easing.SinOut));
            ScrollAnimation.Commit(this, ahScroll, 16, 270, finished: (d, f) => ScrollAnimation = null);
        }

        private void OnCursorChanged(CursorType oldValue, CursorType newValue)
        {
        }

        public virtual bool OnKeyDown(string keyName, KeyModifiers modifiers)
        {
            KeyModifiers = modifiers;
            var args = new CanvasKeyEventArgs(keyName, modifiers);
            KeyDown?.Invoke(this, args);
            return args.Handled;
        }

        protected void OnScrollComplete(double arg1, bool arg2)
        {
            if (HorizontalScrollAnimation != null)
            {
                HorizontalScrollAnimation = null;
                нorizontalScrolledHandler?.Invoke(this, new ScrollEventArgs((int)HorizontalValue));
            }
            if (VerticalScrollAnimation != null)
            {
                VerticalScrollAnimation = null;
                verticalScrolledHandler?.Invoke(this, new ScrollEventArgs((int)VerticalValue));
            }
            ScrollComplete?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool ContainsCaptureBox(double x, double y)
        {
            var baseValue = CheckCaptureBox?.Invoke(x, y) ?? false;
            return baseValue
                || (VerticalScrollBarVisible && GetVerticalValueBounds().Contains((float)x, (float)y))
                || (HorizontalScrollBarVisible && GetHorizontalValueBounds().Contains((float)x, (float)y));
        }

        public Func<double, double, bool> CheckCaptureBox;

    }
}

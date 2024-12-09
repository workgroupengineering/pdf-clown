using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Threading;
using Xamarin.Forms;

namespace PdfClown.UI
{
    public class SKScrollView : SKCanvasView, ISKScrollView
    {
        public static readonly BindableProperty CursorProperty = BindableProperty.Create(nameof(Cursor), typeof(CursorType), typeof(SKScrollView), CursorType.Arrow,
            propertyChanged: (bindable, oldValue, newValue) => ((SKScrollView)bindable).OnCursorChanged((CursorType)oldValue, (CursorType)newValue));

        private const string ahScroll = "ScrollAnimation";
        private const string ahVScroll = "VScrollAnimation";
        private const string ahHScroll = "HScrollAnimation";
        
        private bool isDrawing;
        protected ScrollLogic scroll;

        public SKScrollView()
        {
            Envir.Init();
            IsTabStop = true;
            IgnorePixelScaling = false;
            EnableTouchEvents = true;
            Touch += OnTouch;
            scroll = new ScrollLogic(this);
        }

        public CursorType Cursor
        {
            get => (CursorType)GetValue(CursorProperty);
            set => SetValue(CursorProperty, value);
        }

        public bool AllowScrollView { get; set; } = true;

        public event EventHandler<CanvasKeyEventArgs> KeyDown;

        public double VMaximum
        {
            get => scroll.VMaximum;
            set => scroll.VMaximum = value;
        }

        public double HMaximum
        {
            get => scroll.HMaximum;
            set => scroll.HMaximum =  value;
        }

        public double VValue
        {
            get => scroll.VValue;
            set => scroll.VValue = value;            
        }

        public double HValue
        {
            get => scroll.HValue;
            set => scroll.HValue = value;            
        }

        public KeyModifiers KeyModifiers { get; set; }

        public Animation ScrollAnimation { get; private set; }
        
        public Animation VScrollAnimation { get; private set; }
        
        public Animation HScrollAnimation { get; private set; }

        public bool IsVScrollAnimation => (VScrollAnimation ?? ScrollAnimation) != null;

        public bool IsHScrollAnimation => (HScrollAnimation ?? ScrollAnimation) != null;

        public event EventHandler<SKPaintSurfaceEventArgs> PaintContent;

        public event EventHandler<SKPaintSurfaceEventArgs> PaintOver;        

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
            scroll.OnSizeAllocated(width, height);            
        }

        protected void OnTouch(object sender, SKTouchEventArgs args)
        {
            var e = new TouchEventArgs((TouchAction)args.ActionType, (MouseButton)args.MouseButton);
            OnTouch(e);
            args.Handled = e.Handled;
            //if (ScrollBarVisible)
            //{
            //    var scrollBound = GetScrollBounds();
            //    if (scrollBound.Contains(e.Location))
            //    {
            //        e.Handled = true;
            //    }
            //}            
        }

        protected virtual void OnTouch(TouchEventArgs e)
        {
            scroll.OnTouch(e);
        }

        private void OnTargetScrolled(object sender, ScrollEventArgs e)
        {
            OnScrolled(e.WheelDelta);
        }

        public virtual bool OnScrolled(int delta)
        {
            var temp = VValue;
            VAnimateScroll(VValue - (delta * 1.5), 16, 160);
            return temp != VValue;
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
                scroll.PaintScrollBars(canvas);
                PaintOver?.Invoke(this, e);
            }
            finally
            {
                isDrawing = false;
            }
        }

        protected virtual void OnPaintContent(SKPaintSurfaceEventArgs e)
        {
            PaintContent?.Invoke(this, e);
        }

        public void HAnimateScroll(double newValue, uint rate = DefaultSKStyles.MaxSize, uint legth = 400, Easing asing = null)
        {
            if (HValue == newValue)
                return;
            if (HScrollAnimation != null)
            {
                this.AbortAnimation(ahHScroll);
            }
            newValue = scroll.NormalizeHValue(newValue);
            HScrollAnimation = new Animation(v => HValue = v, HValue, newValue, asing);
            HScrollAnimation.Commit(this, ahHScroll, rate, legth, finished: OnScrollComplete);
        }

        public void VAnimateScroll(double newValue, uint rate = DefaultSKStyles.MaxSize, uint legth = 400, Easing asing = null)
        {
            if (VValue == newValue)
                return;
            if (VScrollAnimation != null)
            {
                this.AbortAnimation(ahVScroll);
            }
            newValue = scroll.NormalizeVValue(newValue);
            VScrollAnimation = new Animation(v => VValue = v, VValue, newValue, asing);
            VScrollAnimation.Commit(this, ahVScroll, rate, legth, finished: OnScrollComplete);
        }

        protected void AnimateScroll(float top, double left)
        {
            if (VValue == top
                && HValue == left)
                return;
            if (ScrollAnimation != null)
            {
                this.AbortAnimation(ahScroll);
            }
            ScrollAnimation = new Animation
            {
                { 0, 1, new Animation(v => VValue = v, VValue, top, Easing.SinOut) },
                { 0, 1, new Animation(v => HValue = v, HValue, left, Easing.SinOut) }
            };
            ScrollAnimation.Commit(this, ahScroll, 16, 270, finished: OnScrollComplete);
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

        protected virtual void OnScrollComplete(double arg1, bool arg2)
        {
            if (HScrollAnimation != null)
            {
                HScrollAnimation = null;
                scroll.RaiseHScroll();
            }
            if (VScrollAnimation != null)
            {
                VScrollAnimation = null;
                scroll.RaiseVScroll();
            }
            if (ScrollAnimation != null)
            {
                ScrollAnimation = null;
                scroll.RaiseHScroll();
                scroll.RaiseVScroll();
            }
            ScrollComplete?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool ContainsCaptureBox(double x, double y)
        {
            var baseValue = CheckCaptureBox?.Invoke(x, y) ?? false;
            return baseValue
                || (scroll.VScrollBarVisible && scroll.GetVValueBounds().Contains((float)x, (float)y))
                || (scroll.HScrollBarVisible && scroll.GetHValueBounds().Contains((float)x, (float)y));
        }

        public Func<double, double, bool> CheckCaptureBox;
        

        public virtual void InvalidatePaint()
        {
            if (Envir.MainContext == SynchronizationContext.Current)
                InvalidateSurface();
            else
                Envir.MainContext.Post(state => InvalidateSurface(), null);
        }

    }
}

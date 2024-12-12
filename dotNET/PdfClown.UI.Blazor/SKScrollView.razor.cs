using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PdfClown.UI.Blazor.Internal;
using PdfClown.UI.Other;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using System.Runtime.Versioning;

namespace PdfClown.UI.Blazor
{
    [SupportedOSPlatform("browser")]
    public partial class SKScrollView : ComponentBase, ISKScrollView, IDisposable
    {

        private const string ahScroll = "ScrollAnimation";
        private const string ahVScroll = "VScrollAnimation";
        private const string ahHScroll = "HScrollAnimation";

#if __FORCE_GL__
        private SKGLView canvasView;
#else
        private SKCanvasView? canvasView;
#endif
        private SKHtmlScrollInterop? interop;
        private CursorType cursor;

        protected ScrollLogic scroll;

        public SKScrollView()
        {
            //EnableTouchEvents = true;
            //this.Tou += OnTouch;
            scroll = new ScrollLogic(this);
        }

        [Inject]
        IJSRuntime JS { get; set; } = null!;

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        [Parameter]
        public string CanvasId { get; set; } = Guid.NewGuid().ToString("N");

        public CursorType Cursor
        {
            get => cursor;
            set
            {
                if (cursor != value)
                {
                    OnCursorChanged(cursor, value);
                }
            }
        }

        public virtual double Height
        {
            get => scroll.Height;
            set => scroll.Height = value;
        }

        public virtual double Width
        {
            get => scroll.Width;
            set => scroll.Width = value;
        }

        public bool AllowScrollView { get; set; } = true;

        public event EventHandler<CanvasKeyEventArgs>? KeyDown;

        public double VMaximum
        {
            get => scroll.VMaximum;
            set => scroll.VMaximum = value;
        }

        public double HMaximum
        {
            get => scroll.HMaximum;
            set => scroll.HMaximum = value;
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

        public bool VBarVisible => scroll.VBarVisible;

        public bool HBarVisible => scroll.HBarVisible;

        public KeyModifiers KeyModifiers { get; set; }

        public Animation? ScrollAnimation { get; private set; }

        public Animation? VScrollAnimation { get; private set; }

        public Animation? HScrollAnimation { get; private set; }

        public bool IsVScrollAnimation => (VScrollAnimation ?? ScrollAnimation) != null;

        public bool IsHScrollAnimation => (HScrollAnimation ?? ScrollAnimation) != null;

#if __FORCE_GL__
        public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintContent;

        public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintOver;
#else
        public event EventHandler<SKPaintSurfaceEventArgs>? PaintContent;

        public event EventHandler<SKPaintSurfaceEventArgs>? PaintOver;
#endif

        public bool WheelTouchSupported { get; set; } = true;

        public event EventHandler? ScrollComplete;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                interop = await SKHtmlScrollInterop.ImportAsync(JS, CanvasId, OnPointerMove, OnSizeAllocated);
                interop.Init();
            }
        }

        public void InvalidatePaint()
        {
            canvasView?.Invalidate();
        }

        protected virtual void OnTouch(TouchEventArgs e)
        {
            if (WheelTouchSupported && e.ActionType == TouchAction.WheelChanged)
            {
                OnScrolled(e.WheelDelta);
                return;
            }
            scroll.OnTouch(e);
        }

        protected virtual void OnSizeAllocated(double width, double height)
        {
            scroll.OnSizeAllocated(width, height);
        }

        public virtual bool OnScrolled(double delta)
        {
            if (delta == 0)
                return false;
            var temp = VValue;
            VAnimateScroll(delta * 1.5, 16, 160);
            return temp != VValue;
        }

#if __FORCE_GL__
        protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
#else
        protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
#endif
        {
            scroll.XScaleFactor = (float)(e.Info.Width / Width);
            scroll.YScaleFactor = (float)(e.Info.Height / Height);

            var canvas = e.Surface.Canvas;

            canvas.Scale(scroll.XScaleFactor, scroll.YScaleFactor);
            canvas.Clear(SKColors.Silver);

            OnPaintContent(e);
            scroll.PaintScrollBars(canvas);
            PaintOver?.Invoke(this, e);
        }

#if __FORCE_GL__
        protected virtual void OnPaintContent(SKPaintGLSurfaceEventArgs e)
#else
        protected virtual void OnPaintContent(SKPaintSurfaceEventArgs e)
#endif
        {
            PaintContent?.Invoke(this, e);
        }

        public void HAnimateScroll(double delta, uint rate = DefaultSKStyles.MaxSize, uint legth = 400, Easing asing = Easing.SinOut)
        {
            if (HScrollAnimation != null)
            {
                AbortAnimation(ahHScroll);
            }
            var newValue = scroll.NormalizeHValue(HValue + delta);
            HScrollAnimation = new Animation(v => HValue = v, HValue, newValue, asing);
            HScrollAnimation.Commit(this, ahHScroll, rate, legth, finished: OnScrollComplete);
        }

        public void VAnimateScroll(double delta, uint rate = DefaultSKStyles.MaxSize, uint legth = 400, Easing asing = Easing.SinOut)
        {
            if (VScrollAnimation != null)
            {
                AbortAnimation(ahVScroll);
            }
            var newValue = scroll.NormalizeVValue(VValue + delta);
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
                AbortAnimation(ahScroll);
            }
            ScrollAnimation = new Animation();
            ScrollAnimation.Add(0, 1, new Animation(v => VValue = v, VValue, top, Easing.SinOut));
            ScrollAnimation.Add(0, 1, new Animation(v => HValue = v, HValue, left, Easing.SinOut));
            ScrollAnimation.Commit(this, ahScroll, 16, 270, finished: OnScrollComplete);
        }

        private void AbortAnimation(string ahScroll)
        {
            Animation.Abort(this, ahScroll);
        }

        private void OnCursorChanged(CursorType oldValue, CursorType newValue)
        {
            cursor = newValue;
            interop?.ChangeCursor(newValue);
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

        private static KeyModifiers GetKeyModifiers(MouseEventArgs e)
        {
            var modifiers = KeyModifiers.None;
            if (e.AltKey)
                modifiers |= KeyModifiers.Alt;
            if (e.CtrlKey)
                modifiers |= KeyModifiers.Ctrl;
            if (e.ShiftKey)
                modifiers |= KeyModifiers.Shift;
            if (e.MetaKey)
                modifiers |= KeyModifiers.Meta;
            return modifiers;
        }

        private static KeyModifiers GetKeyModifiers(KeyboardEventArgs e)
        {
            var modifiers = KeyModifiers.None;
            if (e.AltKey)
                modifiers |= KeyModifiers.Alt;
            if (e.CtrlKey)
                modifiers |= KeyModifiers.Ctrl;
            if (e.ShiftKey)
                modifiers |= KeyModifiers.Shift;
            if (e.MetaKey)
                modifiers |= KeyModifiers.Meta;
            return modifiers;
        }

        private static KeyModifiers GetKeyModifiers(Microsoft.AspNetCore.Components.Web.TouchEventArgs e)
        {
            var modifiers = KeyModifiers.None;
            if (e.AltKey)
                modifiers |= KeyModifiers.Alt;
            if (e.CtrlKey)
                modifiers |= KeyModifiers.Ctrl;
            if (e.ShiftKey)
                modifiers |= KeyModifiers.Shift;
            if (e.MetaKey)
                modifiers |= KeyModifiers.Meta;
            return modifiers;
        }

        private static MouseButton GetMouseButton(MouseEventArgs e)
        {
            var button = e.Button;
            return GetMouseButton(button);
        }

        private static MouseButton GetMouseButton(long button)
        {
            return button switch
            {
                0 => MouseButton.Left,
                1 => MouseButton.Middle,
                2 => MouseButton.Right,
                _ => MouseButton.Unknown,
            };
        }

        private static MouseButton GetMouseButton(Microsoft.AspNetCore.Components.Web.TouchEventArgs e)
        {
            return MouseButton.Left;
        }

        private static SKPoint GetMouseLocation(MouseEventArgs e)
        {
            return new SKPoint((float)e.OffsetX, (float)e.OffsetY);
        }

        private static SKPoint GetMouseLocation(Microsoft.AspNetCore.Components.Web.TouchEventArgs e)
        {
            return new SKPoint((float)e.Touches[0].PageX, (float)e.Touches[0].PageY);
        }

        private void OnMouseWheel(WheelEventArgs e)
        {
            KeyModifiers = GetKeyModifiers(e);
            OnScrolled(e.DeltaY);
        }

        private void OnPointerUp(PointerEventArgs e)
        {
            KeyModifiers = GetKeyModifiers(e);
            OnTouch(new TouchEventArgs(TouchAction.Released, GetMouseButton(e))
            {
                Location = GetMouseLocation(e),
                PointerId = e.PointerId,
                KeyModifiers = KeyModifiers,
            });
        }

        private void OnPointerDown(PointerEventArgs e)
        {
            KeyModifiers = GetKeyModifiers(e);
            OnTouch(new TouchEventArgs(TouchAction.Pressed, GetMouseButton(e))
            {
                Location = GetMouseLocation(e),
                PointerId = e.PointerId,
                KeyModifiers = KeyModifiers,
            });
        }

        private void OnPointerMove(int[] args)
        {
            KeyModifiers = (KeyModifiers)args[4];
            OnTouch(new TouchEventArgs(TouchAction.Moved, GetMouseButton(args[1]))
            {
                Location = new SKPoint(args[2], args[3]),
                PointerId = args[0],
                KeyModifiers = KeyModifiers,
            });
        }

        private void OnPointerMove(PointerEventArgs e)
        {
            KeyModifiers = GetKeyModifiers(e);
            OnTouch(new TouchEventArgs(TouchAction.Moved, GetMouseButton(e))
            {
                Location = GetMouseLocation(e),
                PointerId = e.PointerId,
                KeyModifiers = KeyModifiers,
            });
        }

        private void OnPointerEnter(PointerEventArgs e)
        {
            KeyModifiers = GetKeyModifiers(e);
            OnTouch(new TouchEventArgs(TouchAction.Entered, GetMouseButton(e))
            {
                Location = GetMouseLocation(e),
                PointerId = e.PointerId,
                KeyModifiers = KeyModifiers,
            });
        }

        private void OnPointerLeave(PointerEventArgs e)
        {
            KeyModifiers = GetKeyModifiers(e);
            OnTouch(new TouchEventArgs(TouchAction.Exited, GetMouseButton(e))
            {
                Location = GetMouseLocation(e),
                PointerId = e.PointerId,
                KeyModifiers = KeyModifiers,
            });
        }

        private void OnKeyDown(KeyboardEventArgs e)
        {
            OnKeyDown(e.Key, GetKeyModifiers(e));
        }

        public void Dispose()
        {
            interop?.DeInit();
            interop?.Dispose();
            interop = null;
        }

        public void CapturePointer(long pointerId)
        {
            interop?.SetCapture((int)pointerId);
        }
    }
}
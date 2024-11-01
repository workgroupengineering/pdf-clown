﻿using PdfClown.UI;
using PdfClown.UI.WPF;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiaSharp.Views.WPF;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(SKScrollView), typeof(SKScrollViewRenderer))]
namespace PdfClown.UI.WPF
{
    public class SKScrollViewRenderer : SKCanvasViewRenderer
    {
        private bool pressed;

        protected override void OnElementChanged(ElementChangedEventArgs<SKCanvasView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement is SKScrollView)
            {
                e.NewElement.Touch -= OnElementTouch;
            }
            if (e.NewElement is SKScrollView scrollView)
            {
                scrollView.WheelTouchSupported = false;
                scrollView.Touch += OnElementTouch;
                if (Control != null)
                {
                    Control.Focusable = Element.IsTabStop;
                    Control.Loaded += OnControlLoaded;
                }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (string.Equals(e.PropertyName, SKScrollView.CursorProperty.PropertyName, StringComparison.Ordinal))
            {
                UpdateCursor();
            }
        }

        private void UpdateCursor()
        {
            if (Control != null && Element is SKScrollView canvas)
            {
                switch (canvas.Cursor)
                {
                    case UI.CursorType.Arrow:
                        Control.Cursor = Cursors.Arrow;
                        break;
                    case UI.CursorType.SizeWE:
                        Control.Cursor = Cursors.SizeWE;
                        break;
                    case UI.CursorType.SizeNS:
                        Control.Cursor = Cursors.SizeNS;
                        break;
                    case UI.CursorType.SizeNESW:
                        Control.Cursor = Cursors.SizeNESW;
                        break;
                    case UI.CursorType.SizeNWSE:
                        Control.Cursor = Cursors.SizeNWSE;
                        break;
                    case UI.CursorType.Hand:
                        Control.Cursor = Cursors.Hand;
                        break;
                    case UI.CursorType.Wait:
                        Control.Cursor = Cursors.Wait;
                        break;
                    case UI.CursorType.ScrollAll:
                        Control.Cursor = Cursors.ScrollAll;
                        break;
                    case UI.CursorType.Cross:
                        Control.Cursor = Cursors.Cross;
                        break;
                    case UI.CursorType.IBeam:
                        Control.Cursor = Cursors.IBeam;
                        break;
                }
            }
        }

        private void OnElementTouch(object sender, SKTouchEventArgs e)
        {
            if (Element is SKScrollView scrollView)
            {
                scrollView.KeyModifiers = GetModifiers();
            }
            if (e.ActionType == SKTouchAction.Released)
            {
                Element.Focus();
                Control.Focus();
            }
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {            
            Control.Loaded -= OnControlLoaded;
            Control.MouseWheel += OnControlMouseWheel;
            Control.PreviewKeyDown += OnControlKeyDown;
            Control.PreviewMouseLeftButtonDown += OnControlMouseLeftButtonDown;
            Control.PreviewMouseLeftButtonUp += OnControlMouseLeftButtonUp;
            Control.PreviewMouseMove += OnControlMouseMove;
        }

        private void OnControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Element is SKScrollView scrollView)
            {
                scrollView.KeyModifiers = GetModifiers();
                scrollView.OnScrolled(e.Delta);
            }
        }

        private void OnControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var view = sender as FrameworkElement;
            var pointerPoint = e.MouseDevice.GetPosition(view);
            if (Element is SKScrollView scrollView)
            {
                scrollView.KeyModifiers = GetModifiers();
                if (scrollView.ContainsCaptureBox(pointerPoint.X, pointerPoint.Y))
                {
                    pressed = true;
                    Mouse.Capture(Control);
                }
            }
        }

        private static KeyModifiers GetModifiers()
        {
            var keyModifiers = KeyModifiers.None;
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                keyModifiers |= KeyModifiers.Alt;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                keyModifiers |= KeyModifiers.Ctrl;
            }
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                keyModifiers |= KeyModifiers.Shift;
            }

            return keyModifiers;
        }

        private void OnControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Element is SKScrollView scrollView)
            {
                scrollView.KeyModifiers = GetModifiers();
            }
            if (pressed)
            {
                RaiseTouch(sender, e, SKTouchAction.Moved);
            }
        }

        private void OnControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (pressed)
            {
                pressed = false;
                Mouse.Capture(null);
                e.Handled = true;
                RaiseTouch(sender, e, SKTouchAction.Released);
            }
        }

        private void RaiseTouch(object sender, MouseEventArgs e, SKTouchAction action)
        {
            var view = sender as FrameworkElement;
            var pointerPoint = e.MouseDevice.GetPosition(view);
            var skPoint = GetScaledCoord(pointerPoint.X, pointerPoint.Y);
            var args = new SKTouchEventArgs(e.Timestamp, action, SKMouseButton.Left, SKTouchDeviceType.Mouse, skPoint, true);
            ((ISKCanvasViewController)Element).OnTouch(args);
        }

        private void OnControlKeyDown(object sender, KeyEventArgs e)
        {
            if (Element is SKScrollView scrollView)
            {
                scrollView.KeyModifiers = GetModifiers();
                e.Handled = scrollView.OnKeyDown(e.Key.ToString(), scrollView.KeyModifiers);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Element != null)
                {
                    Element.Touch -= OnElementTouch;
                }
                if (Control != null)
                {
                    Control.Loaded -= OnControlLoaded;
                    Control.MouseWheel -= OnControlMouseWheel;
                    Control.PreviewKeyDown -= OnControlKeyDown;
                    Control.PreviewMouseLeftButtonDown -= OnControlMouseLeftButtonDown;
                    Control.PreviewMouseLeftButtonUp -= OnControlMouseLeftButtonUp;
                    Control.PreviewMouseMove -= OnControlMouseMove;
                }
            }
            base.Dispose(disposing);
        }

        public SKPoint GetScaledCoord(double x, double y)
        {
            if (Element.IgnorePixelScaling)
            {
                return new SKPoint((float)x, (float)y);
            }
            else
            {
                return SKCanvasHelper.GetScaledCoord(Control, x, y);
            }
        }
    }

    public static class SKCanvasHelper
    {
        public static SKPoint GetScaledCoord(FrameworkElement control, double x, double y)
        {
            var matrix = control != null ? PresentationSource.FromVisual(control)?.CompositionTarget?.TransformToDevice : null;
            var xfactor = matrix?.M11 ?? 1D;
            var yfactor = matrix?.M22 ?? 1D;
            x = x * xfactor;
            y = y * yfactor;

            return new SKPoint((float)x, (float)y);
        }
    }
}

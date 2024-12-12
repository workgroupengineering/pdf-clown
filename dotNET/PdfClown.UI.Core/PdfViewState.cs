using PdfClown.UI.Operations;
using SkiaSharp;
using System;

namespace PdfClown.UI
{
    public class PdfViewState : EventArgs
    {
        private PdfPageViewModel? pageView;
        private SKPoint pointerLocation;
        private SKMatrix navigationMatrix = SKMatrix.Identity;
        private SKMatrix windowScaleMatrix = SKMatrix.Identity;

        public PdfViewState(IPdfView viewer)
        {
            Viewer = viewer;
        }

        //Common
        public readonly IPdfView Viewer;

        public EditorOperations Operations => Viewer.Operations;

        public void SetWindowScale(float xScale, float yScale)
        {
            if (XScaleFactor != xScale
                || YScaleFactor != yScale)
            {
                WindowScaleMatrix = SKMatrix.CreateScale(XScaleFactor, YScaleFactor);
            }
        }

        public float XScaleFactor
        {
            get => WindowScaleMatrix.ScaleX;
        }

        public float YScaleFactor
        {
            get => WindowScaleMatrix.ScaleY;
        }

        public SKMatrix ScaleMatrix = SKMatrix.Identity;

        public SKMatrix WindowScaleMatrix
        {
            get => windowScaleMatrix;
            set
            {
                if (windowScaleMatrix != value)
                {
                    windowScaleMatrix = value;
                    windowScaleMatrix.TryInvert(out InvertWindowScaleMatrix);
                }
            }
        }

        public SKMatrix InvertWindowScaleMatrix;

        public SKMatrix NavigationMatrix
        {
            get => navigationMatrix;
            set
            {
                if (navigationMatrix != value)
                {
                    navigationMatrix = value;
                    NavigationMatrix.TryInvert(out InvertNavigationMatrix);

                    ViewMatrix = NavigationMatrix.PostConcat(windowScaleMatrix);
                    ViewMatrix.TryInvert(out InvertViewMatrix);

                    NavigationArea = InvertNavigationMatrix.MapRect(WindowArea);
                }
            }
        }
        public SKMatrix InvertNavigationMatrix = SKMatrix.Identity;
        public SKMatrix ViewMatrix = SKMatrix.Identity;
        public SKMatrix InvertViewMatrix = SKMatrix.Identity;

        public SKRect WindowArea;
        public SKRect NavigationArea;

        public PdfPageViewModel? PageView
        {
            get => pageView;
            set
            {
                pageView = value;
                if (pageView != null)
                {
                    PageViewMatrix = pageView.GetViewMatrix(this);
                    PageViewMatrix.TryInvert(out InvertPageViewMatrix);

                    PageMatrix = PageViewMatrix.PreConcat(pageView.Page.RotateMatrix);
                    PageMatrix.TryInvert(out InvertPageMatrix);

                    PagePointerLocation = InvertPageViewMatrix.MapPoint(PointerLocation);
                }
            }
        }

        public SKMatrix PageViewMatrix = SKMatrix.Identity;
        public SKMatrix InvertPageViewMatrix = SKMatrix.Identity;
        public SKMatrix PageMatrix = SKMatrix.Identity;
        public SKMatrix InvertPageMatrix = SKMatrix.Identity;

        //Touch
        public TouchAction TouchAction;
        public MouseButton TouchButton;
        public SKPoint PointerLocation
        {
            get => pointerLocation;
            set
            {
                if (pointerLocation != value)
                {
                    pointerLocation = value;
                    ViewPointerLocation = InvertViewMatrix.MapPoint(pointerLocation);
                }
            }
        }

        public SKPoint ViewPointerLocation;
        
        public SKPoint PagePointerLocation;
        
        public SKPoint? PressedLocation;

        public float Scale
        {
            get => ScaleMatrix.ScaleX;
            set
            {
                if (value != Scale)
                {
                    ScaleMatrix.ScaleX = value;
                    ScaleMatrix.ScaleY = value;
                }
            }
        }

    }

}

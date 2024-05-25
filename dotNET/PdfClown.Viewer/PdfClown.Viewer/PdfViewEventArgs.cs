using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Viewer.ToolTip;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;

namespace PdfClown.Viewer
{
    public class PdfViewEventArgs : EventArgs
    {
        private PdfPageView pageView;
        private SKPoint pointerLocation;
        private SKMatrix navigationMatrix = SKMatrix.Identity;
        private SKMatrix windowScaleMatrix = SKMatrix.Identity;
        private Annotation toolTipAnnotation;
        private MarkupToolTipRenderer markupToolTipRenderer;
        private LinkToolTipRenderer linkToolTipRenderer;

        //Common
        public PdfView Viewer;
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
                }
            }
        }
        public SKMatrix InvertNavigationMatrix = SKMatrix.Identity;
        public SKMatrix ViewMatrix = SKMatrix.Identity;
        public SKMatrix InvertViewMatrix = SKMatrix.Identity;

        public SKRect WindowArea;
        public SKRect Area;

        public PdfPageView PageView
        {
            get => pageView;
            set
            {
                pageView = value;
                if (pageView != null)
                {
                    PageViewMatrix = ViewMatrix.PreConcat(pageView.Matrix);
                    PageViewMatrix.TryInvert(out InvertPageViewMatrix);

                    PageMatrix = PageViewMatrix.PreConcat(pageView.Page.RotateMatrix);
                    PageMatrix.TryInvert(out InvertPageMatrix);
                }
            }
        }

        public SKMatrix PageViewMatrix = SKMatrix.Identity;
        public SKMatrix InvertPageViewMatrix = SKMatrix.Identity;
        public SKMatrix PageMatrix = SKMatrix.Identity;
        public SKMatrix InvertPageMatrix = SKMatrix.Identity;


        public PdfPage Page => PageView.Page;

        //Touch
        public SKTouchEventArgs TouchEvent;

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

        public SKPoint MoveLocation;
        public SKPoint PagePointerLocation;
        public SKPoint? PressedLocation;

        public Annotation Annotation;
        public SKRect AnnotationBound;
        public Annotation DrawAnnotation;
        public SKRect DrawAnnotationBound;

        public SKRect ToolTipBounds;       

        public Annotation ToolTipAnnotation
        {
            get => toolTipAnnotation;
            set
            {
                if (toolTipAnnotation != value)
                {
                    toolTipAnnotation = value;
                    if (toolTipAnnotation is Markup markup)
                    {
                        ToolTipRenderer = markupToolTipRenderer ??= new MarkupToolTipRenderer();
                        markupToolTipRenderer.Markup = markup;
                        ToolTipBounds = markupToolTipRenderer.GetWindowBound(this);
                    }
                    else if (toolTipAnnotation is Link link)
                    {
                        ToolTipRenderer = linkToolTipRenderer ??= new LinkToolTipRenderer();
                        linkToolTipRenderer.Link = link;
                        ToolTipBounds = linkToolTipRenderer.GetWindowBound(this);
                    }
                    else
                    {
                        ToolTipRenderer = null;
                    }
                    Viewer.InvalidateSurface();
                }
            }
        }

        public ToolTipRenderer ToolTipRenderer;

        //Draw
        public SKCanvas Canvas;

    }
}

using PdfClown.Documents;
using PdfClown.Documents.Contents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Util.Math.Geom;
using PdfClown.UI.ToolTip;
using SkiaSharp;
using System;
using System.Linq;

namespace PdfClown.UI
{
    public class PdfViewState : EventArgs
    {
        private PdfPageViewModel pageView;
        private SKPoint pointerLocation;
        private SKMatrix navigationMatrix = SKMatrix.Identity;
        private SKMatrix windowScaleMatrix = SKMatrix.Identity;
        private Annotation toolTipAnnotation;
        private MarkupToolTipRenderer markupToolTipRenderer;
        private LinkToolTipRenderer linkToolTipRenderer;

        //Common
        public IPdfView Viewer;

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

        public PdfPageViewModel PageView
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
                }
            }
        }

        public SKMatrix PageViewMatrix = SKMatrix.Identity;
        public SKMatrix InvertPageViewMatrix = SKMatrix.Identity;
        public SKMatrix PageMatrix = SKMatrix.Identity;
        public SKMatrix InvertPageMatrix = SKMatrix.Identity;

        public PdfPage Page => PageView.Page;

        //Touch
        //public SKTouchEventArgs TouchEvent;
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

        public IPdfPageViewModel GetCenterPage()
        {
            if (Viewer.Document == null)
                return null;
            var doc = Viewer.Document;
            var area = WindowArea;
            area.Inflate(-area.Width / 3F, -area.Height / 3F);
            area = InvertNavigationMatrix.MapRect(area);
            for (int i = GetDisplayPageIndex(); i < doc.PagesCount; i++)
            {
                var pageView = doc[i];
                if (pageView.Bounds.IntersectsWith(area))
                {
                    return pageView;
                }
            }
            return doc.PageViews.FirstOrDefault();
        }

        public int GetDisplayPageIndex()
        {
            var verticalValue = -(NavigationMatrix.TransY);
            var page = Viewer.Document.PageViews.FirstOrDefault(p => (p.Bounds.Bottom * NavigationMatrix.ScaleY) > verticalValue);
            return page?.Order ?? 0;
        }

        public void Draw(SKCanvas canvas)
        {
            try
            {
                Canvas = canvas;
                Canvas.Save();
                Canvas.SetMatrix(ViewMatrix);
                var doc = Viewer.Document;
                for (int i = GetDisplayPageIndex(); i < doc.PagesCount; i++)
                {
                    var pageView = doc[i];
                    var pageBounds = pageView.Bounds;
                    if (pageBounds.IntersectsWith(NavigationArea))
                    {
                        pageView.Draw(this);
                    }
                    else if (pageBounds.Top > NavigationArea.Bottom)
                    {
                        break;
                    }
                }
                DrawTextSelection();
                //DrawSelectionRect();
                DrawAnnotationToolTip();
                Canvas.Restore();
            }
            finally
            {
                Canvas = null;
            }
        }

        private void DrawTextSelection()
        {
            if (!Viewer.TextSelection.Any())
            {
                return;
            }
            IContentContext context = null;
            PdfPageViewModel pageView = null;
            foreach (var textChar in Viewer.TextSelection.Chars)
            {
                if (context != textChar.TextString.Context)
                {
                    context = textChar.TextString.Context;
                    pageView = Viewer.Document.GetPageView(context as PdfPage);
                    if (pageView != null)
                    {
                        Canvas.SetMatrix(pageView.GetViewMatrix(this));
                    }
                }
                if (pageView == null
                    || !pageView.Bounds.IntersectsWith(NavigationArea))
                    continue;
                //if (textChar.B.TextString.Context == pageView.Page)
                {
                    using (var path = textChar.Quad.GetPath())
                    {
                        Canvas.DrawPath(path, DefaultSKStyles.PaintTextSelectionFill);
                    }
                }
            }
        }

        private void DrawSelectionRect(Quad selectionRect)
        {
            if (selectionRect.Width > 0
                && selectionRect.Height > 0)
            {
                Canvas.DrawRect(selectionRect.GetBounds(), DefaultSKStyles.PaintSelectionRect);
            }
        }

        private void DrawAnnotationToolTip()
        {
            if (ToolTipAnnotation != null)
            {
                var pageView = Viewer.Document.GetPageView(ToolTipAnnotation.Page);
                if (pageView.Bounds.IntersectsWith(NavigationArea))
                {
                    ToolTipRenderer?.Draw(this);
                }
            }
        }

        public bool Touch()
        {
            var doc = Viewer.Document;
            for (int i = GetDisplayPageIndex(); i < doc.PagesCount; i++)
            {
                var pageView = doc[i];
                if (pageView.Bounds.Contains(ViewPointerLocation))
                {
                    pageView.Touch(this);
                    return true;
                }
            }
            return false;
        }
    }
}

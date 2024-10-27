using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.UI.Operations;
using PdfClown.UI.ToolTip;
using PdfClown.Util.Math;
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
        private float xScaleFactor = 1F;
        private float yScaleFactor = 1F;
        private float scale = 1;
        private IPdfDocumentViewModel document;
        private IPdfPageViewModel currentPage;
        private IPdfPageViewModel newPage;

        private PdfPageEventArgs cacheCurrentArgs = new(null);
        private FloatEventArgs cacheScaleArgs = new(0F);


        //Common
        public IPdfView Viewer;

        public EditOperationList Operations => Viewer?.Operations;

        public IPdfDocumentViewModel Document
        {
            get => document;
            set
            {
                if (document == value)
                    return;

                if (document != null)
                {
                    document.BoundsChanged -= OnDocumentBoundsChanged;
                }
                Operations.Document = value;

                ToolTipAnnotation = null;
                document = value;
                UpdateMaximums();
                PagesCount = document?.PagesCount ?? 0;
                if (document != null)
                {
                    document.BoundsChanged += OnDocumentBoundsChanged;
                    CurrentPage = document.PageViews.FirstOrDefault();
                    Viewer.ScrollTo(CurrentPage);
                }
                else
                {
                    Viewer.InvalidatePaint();
                }
            }
        }

        public IPdfPageViewModel CurrentPage
        {
            get
            {
                if (currentPage == null
                    || currentPage.Document != Document)
                {
                    CurrentPage = GetCenterPage();
                }
                return currentPage;
            }
            set
            {
                if (currentPage != value)
                {
                    OnCurrentPageChanged(value);
                }
            }
        }

        public int CurrentPageNumber
        {
            get => (CurrentPage?.Index ?? -1) + 1;
            set
            {
                if (Document == null
                    || PagesCount == 0)
                {
                    return;
                }
                int index = GetPageIndex(value);
                if ((index + 1) != CurrentPageNumber)
                {
                    CurrentPage = Document[index];
                }
            }
        }

        public IPdfPageViewModel NewPage
        {
            get => newPage;
            private set
            {
                if (newPage != value)
                {
                    newPage = value;
                    if (newPage != null)
                        Viewer.ScrollTo(newPage);
                }
            }
        }

        public int NewPageNumber
        {
            get => NewPage != null
                ? NewPage.Index + 1
                : CurrentPageNumber;
            set
            {
                if (Document == null
                    || PagesCount == 0)
                {
                    return;
                }
                int index = GetPageIndex(value);
                if ((index + 1) != CurrentPageNumber)
                {
                    NewPage = Document[index];
                }
            }
        }

        public float XScaleFactor
        {
            get => xScaleFactor;
            set
            {
                if (xScaleFactor != value)
                {
                    xScaleFactor = value;
                    OnWindowScaleChanged();
                }
            }
        }

        public float YScaleFactor
        {
            get => yScaleFactor;
            set
            {
                if (yScaleFactor != value)
                {
                    yScaleFactor = value;
                    OnWindowScaleChanged();
                }
            }
        }

        public bool VerticalScrollBarVisible => Viewer.VerticalMaximum >= (Viewer.Height + DefaultSKStyles.StepSize);

        public bool HorizontalScrollBarVisible => Viewer.HorizontalMaximum >= (Viewer.Width + DefaultSKStyles.StepSize);

        public SKMatrix ScaleMatrix;

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

        public PdfPage Page => PageView?.Page;

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
                        linkToolTipRenderer?.Free();
                    }
                    else if (toolTipAnnotation is Link link)
                    {
                        ToolTipRenderer = linkToolTipRenderer ??= new LinkToolTipRenderer();
                        linkToolTipRenderer.Link = link;
                        ToolTipBounds = linkToolTipRenderer.GetWindowBound(this);
                        markupToolTipRenderer?.Free();
                    }
                    else
                    {
                        markupToolTipRenderer?.Free();
                        linkToolTipRenderer?.Free();
                        ToolTipRenderer = null;
                    }
                    Viewer.InvalidatePaint();
                }
            }
        }

        public float Scale
        {
            get => scale;
            set
            {
                if (value != scale)
                {
                    OnScaleContentChanged(scale, value);
                }
            }
        }

        public int PagesCount { get; private set; }

        public ToolTipRenderer ToolTipRenderer;

        //Draw
        public SKCanvas Canvas;

        public event PdfPageEventHandler CurrentPageChanged;

        public event FloatEventHandler ScaleChanged;

        private void OnCurrentPageChanged(IPdfPageViewModel page)
        {
            if (newPage != null && newPage == page)
            {
                NewPage = null;
            }
            cacheCurrentArgs.Page =
                currentPage = page;
            CurrentPageChanged?.Invoke(cacheCurrentArgs);
        }

        public IPdfPageViewModel GetCenterPage()
        {
            if (Document == null)
                return null;
            var doc = Document;
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
            var page = Document.PageViews.FirstOrDefault(p => (p.Bounds.Bottom * NavigationMatrix.ScaleY) > verticalValue);
            return page?.Order ?? 0;
        }

        public void Draw(SKCanvas canvas)
        {
            try
            {
                Canvas = canvas;
                Canvas.Save();
                Canvas.SetMatrix(ViewMatrix);
                for (int i = GetDisplayPageIndex(); i < document.PagesCount; i++)
                {
                    var pageView = document[i];
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
                //DrawSelectionRect();
                DrawAnnotationToolTip();
                Canvas.Restore();
            }
            finally
            {
                Canvas = null;
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
                var pageView = Document.GetPageView(ToolTipAnnotation.Page);
                if (pageView?.Bounds.IntersectsWith(NavigationArea) ?? false)
                {
                    ToolTipRenderer?.Draw(this);
                }
            }
        }

        public bool Touch()
        {
            for (int i = GetDisplayPageIndex(); i < document.PagesCount; i++)
            {
                var pageView = document[i];
                if (pageView.Bounds.Contains(ViewPointerLocation))
                {
                    pageView.Touch(this);
                    return true;
                }
            }
            return false;
        }

        protected void OnWindowScaleChanged()
        {
            WindowScaleMatrix = SKMatrix.CreateScale(XScaleFactor, YScaleFactor);
        }

        public void UpdateMaximums()
        {
            UpdateCurrentMatrix();
            var size = Document?.Size ?? SKSize.Empty;
            Viewer.HorizontalMaximum = size.Width * scale;
            Viewer.VerticalMaximum = size.Height * scale;
            Viewer.InvalidatePaint();
        }

        public void UpdateCurrentMatrix() => UpdateCurrentMatrix((float)Viewer.Width, (float)Viewer.Height);

        public void UpdateCurrentMatrix(float width, float height)
        {
            var horizontalValue = (float)Viewer.HorizontalValue;
            var verticalValue = (float)Viewer.VerticalValue;
            var size = Document?.Size ?? SKSize.Empty;
            WindowArea = SKRect.Create(0, 0, width, height);
            var maximumWidth = size.Width * scale;
            var maximumHeight = size.Height * scale;
            var dx = 0F; var dy = 0F;
            if (maximumWidth < WindowArea.Width)
            {
                dx = (float)((width - 10) - maximumWidth) / 2;
            }

            if (maximumHeight < WindowArea.Height)
            {
                dy = (float)(height - maximumHeight) / 2;
            }
            NavigationMatrix = new SKMatrix(
                scale, 0, (-horizontalValue) + dx,
                0, scale, (-verticalValue) + dy,
                0, 0, 1);
        }

        public SKPoint ScrollTo(IPdfPageViewModel page)
        {
            if (page == null || Document == null)
            {
                return SKPoint.Empty;
            }

            switch (Viewer.FitMode)
            {
                case PdfViewFitMode.MaxWidth:
                    Scale = (float)WindowArea.Width / Document.Size.Width;
                    break;
                case PdfViewFitMode.PageHeight:
                    Scale = (float)WindowArea.Height / (page.Bounds.Height + 6);
                    break;
                case PdfViewFitMode.PageWidth:
                    Scale = (float)WindowArea.Width / (page.Bounds.Width + 20);
                    break;
                case PdfViewFitMode.PageSize:
                    var vScale = (float)WindowArea.Height / (page.Bounds.Height + 6);
                    var hScale = (float)WindowArea.Width / (page.Bounds.Width + 20);
                    Scale = hScale < vScale ? hScale : vScale;
                    break;
            }

            var bound = ScaleMatrix.MapRect(page.Bounds);
            return new SKPoint(bound.Left - (WindowArea.MidX - bound.Width / 2),
                               bound.Height <= WindowArea.Height
                                ? bound.Top - (WindowArea.MidY - bound.Height / 2)
                                : bound.Top - 5);
        }

        public SKPoint ScrollTo(Annotation annotation)
        {
            if (annotation?.Page == null)
            {
                return SKPoint.Empty;
            }

            var pageView = Document.GetPageView(annotation.Page);
            if (pageView == null)
            {
                return SKPoint.Empty;
            }
            var matrix = SKMatrix.CreateScale(scale, scale)
                .PreConcat(pageView.Matrix)
                .PreConcat(pageView.Document.Matrix);
            var bound = annotation.GetViewBounds(matrix);
            return new SKPoint(bound.Left - (WindowArea.MidX - bound.Width / 2),
                               bound.Top - (WindowArea.MidY - bound.Height / 2));
        }

        public void ScaleToPointer(float delta) => ScaleToPointer(delta, PointerLocation);

        public void ScaleToPointer(float delta, SKPoint pointerLocation)
        {
            var scaleStep = 0.06F * Math.Sign(delta);
            var newScale = scale + scaleStep + scaleStep * scale;
            if (newScale < 0.01F)
                newScale = 0.01F;
            if (newScale > 60F)
                newScale = 60F;
            if (newScale != scale)
            {
                var unscaleLocations = new SKPoint(pointerLocation.X / XScaleFactor, pointerLocation.Y / YScaleFactor);
                var oldSpacePoint = InvertNavigationMatrix.MapPoint(unscaleLocations);

                Scale = newScale;

                var newCurrentLocation = NavigationMatrix.MapPoint(oldSpacePoint);

                var vector = newCurrentLocation - unscaleLocations;
                if (HorizontalScrollBarVisible)
                {
                    Viewer.HorizontalValue += vector.X;
                }
                if (VerticalScrollBarVisible)
                {
                    Viewer.VerticalValue += vector.Y;
                }
            }
        }

        public void OnTouch(TouchAction actionType, MouseButton mouseButton, SKPoint location)
        {
            TouchAction = actionType;
            TouchButton = mouseButton;
            PointerLocation = location;
            try
            {
                if (TouchButton == MouseButton.Middle
                    && TouchDrag())
                {
                    return;
                }
                if (Document == null
                    || !Document.IsPaintComplete)
                {
                    return;
                }
                if (Touch())
                {
                    return;
                }
                if (Viewer.Operations.Current != OperationType.AnnotationDrag)
                    Viewer.Cursor = CursorType.Arrow;
                PageView = null;
            }
            finally
            {
                if (CanDragByPointer())
                {
                    TouchDrag();
                }
            }
        }

        private bool CanDragByPointer() => Viewer.ScrollByPointer
                && TouchButton == MouseButton.Left
                && Viewer.Cursor == CursorType.Arrow;

        private bool TouchDrag()
        {
            if (TouchAction == TouchAction.Pressed)
            {
                MoveLocation = PointerLocation;
                if (TouchButton == MouseButton.Middle)
                    Viewer.Cursor = CursorType.ScrollAll;
                return true;
            }
            else if (TouchAction == TouchAction.Moved)
            {
                var vector = PointerLocation - MoveLocation;
                Viewer.HorizontalValue -= vector.X;
                Viewer.VerticalValue -= vector.Y;
                MoveLocation = PointerLocation;
                return true;
            }
            return false;
        }

        private void OnDocumentBoundsChanged(object sender, EventArgs e)
        {
            UpdateMaximums();
        }

        private int GetPageIndex(int value)
        {
            var index = value - 1;
            if (index < 0)
            {
                index = PagesCount - 1;
            }
            else if (index >= PagesCount)
            {
                index = 0;
            }

            return index;
        }

        private void OnScaleContentChanged(float oldValue, float newValue)
        {
            scale = newValue;
            ScaleMatrix = SKMatrix.CreateScale(Scale, Scale);

            UpdateMaximums();
            cacheScaleArgs.Value = newValue;
            ScaleChanged?.Invoke(cacheScaleArgs);
        }
    }

}

using Microsoft.AspNetCore.Components;
using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.UI.Operations;
using SkiaSharp.Views.Blazor;

namespace PdfClown.UI.Blazor
{
    public partial class PdfView : SKScrollView, IPdfView
    {
        private readonly PdfViewState state;
        private ControlPoint selectedPoint;

        private bool showCharBound;
        private PdfViewFitMode fitMode = PdfViewFitMode.PageSize;
        private bool showMarkup = true;
        private Annotation hoverAnnotation;
        private bool isReadOnly;
        private Markup selectedMarkup;
        private ControlPoint hoverPoint;

        public PdfView()
        {
            Envir.Init();
            state = new PdfViewState { Viewer = this };

            TextSelection = new TextSelection();
            TextSelection.Changed += OnTextSelectionChanged;

            Operations = new EditOperationList { Viewer = this };
            Operations.Changed += OnOperationsChanged;            
        }

        [Parameter]
        public PdfViewFitMode FitMode
        {
            get => fitMode;
            set
            {
                if (fitMode != value)
                {
                    OnFitModeChanged(fitMode, value);
                }
            }
        }

        [Parameter]
        public float ScaleContent
        {
            get => state.ScaleContent;
            set
            {
                if (ScaleContent != value)
                {
                    OnScaleContentChanged(state.ScaleContent, value);
                }
            }
        }

        [Parameter]
        public bool ShowMarkup
        {
            get => showMarkup;
            set
            {
                if (showMarkup != value)
                {
                    OnShowMarkupChanged(showMarkup, value);
                }
            }
        }

        public Annotation SelectedAnnotation
        {
            get => Operations.Annotation;
            set
            {
                if (SelectedAnnotation != value)
                {
                    OnSelectedAnnotationChanged(Operations.Annotation, value);
                }
            }
        }

        public Annotation HoverAnnotation
        {
            get => hoverAnnotation;
            set
            {
                if (hoverAnnotation != value)
                {
                    OnHoverAnnotationChanged(hoverAnnotation, value);
                }
            }
        }

        public Markup SelectedMarkup
        {
            get => selectedMarkup;
            set
            {
                if (selectedMarkup != value)
                {
                    OnSelectedMarkupChanged(selectedMarkup, value);
                }
            }
        }

        public ControlPoint SelectedPoint
        {
            get => selectedPoint;
            set
            {
                if (selectedPoint != value)
                {
                    OnSelectedPointChanged(selectedPoint, value);
                }
            }
        }

        public ControlPoint HoverPoint
        {
            get => hoverPoint;
            set
            {
                if (hoverPoint != value)
                {
                    OnHoverPointChanged(hoverPoint, value);
                }
            }
        }

        [Parameter]
        public bool IsReadOnly
        {
            get => isReadOnly;
            set => OnIsReadOnlyChanged(isReadOnly, value);
        }

        [Parameter]
        public bool ShowCharBound
        {
            get => showCharBound;
            set
            {
                if (ShowCharBound != value)
                {
                    OnShowCharBoundChanged(showCharBound, value);
                }
            }
        }

        public IPdfDocumentViewModel Document
        {
            get => state.Document;
            set
            {
                if (state.Document != value)
                {
                    state.Document = value;
                    DocumentChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public PdfPage CurrentPage
        {
            get => Page?.GetPage(state);
            set => Page = Document.GetPageView(value);
        }

        public IPdfPageViewModel Page
        {
            get => state.CurrentPage;
            set
            {
                if (state.CurrentPage != value)
                {
                    state.CurrentPage = value;
                }
            }
        }

        public TextSelection TextSelection { get; private set; }

        public EditOperationList Operations { get; private set; }

        public bool IsChanged => Operations.HashOperations;

        public int PagesCount
        {
            get => Document?.PagesCount ?? 0;
            set { }
        }

        public int PageNumberWithScroll
        {
            get => PageNumber;
            set
            {
                if (PageNumber != value)
                {
                    PageNumber = value <= 0 ? 1 : value > PagesCount ? PagesCount : value;
                    ScrollTo(Page);
                }
            }
        }

        public int PageNumber
        {
            get => (Page?.Index ?? -1) + 1;
            set
            {
                if (Document == null
                    || PagesCount == 0)
                {
                    return;
                }
                var index = value - 1;
                if (index < 0)
                {
                    index = PagesCount - 1;
                }
                else if (index >= PagesCount)
                {
                    index = 0;
                }
                if ((index + 1) != PageNumber)
                {
                    Page = Document[index];
                }
            }
        }

        public event EventHandler<AnnotationEventArgs> SelectedAnnotationChanged;

        public event EventHandler<EventArgs> DocumentChanged;

        public void NextPage() => PageNumberWithScroll += 1;

        private bool CanNextPage() => PageNumber < PagesCount;

        public void PrevPage() => PageNumberWithScroll -= 1;

        private bool CanPrevPage() => PageNumber > 1;

        private void OnFitModeChanged(PdfViewFitMode oldValue, PdfViewFitMode newValue)
        {
            fitMode = newValue;
            //InvalidateSurface();
            ScrollTo(Page);
        }

        private void OnScaleContentChanged(float oldValue, float newValue)
        {
            state.ScaleContent = newValue;
        }

        private void OnShowMarkupChanged(bool oldValue, bool newValue)
        {
            showMarkup = newValue;
            InvalidatePaint();
        }

        private void OnHoverAnnotationChanged(Annotation oldValue, Annotation newValue)
        {
            hoverAnnotation = newValue;
        }

        private void OnSelectedAnnotationChanged(Annotation oldValue, Annotation newValue)
        {
            Operations.Annotation = newValue;
            SelectedMarkup = newValue as Markup;
            SelectedAnnotationChanged?.Invoke(this, new AnnotationEventArgs(newValue));
            InvalidatePaint();
        }

        private void OnSelectedMarkupChanged(Markup oldValue, Markup newValue)
        {
            selectedMarkup = newValue;
            SelectedAnnotation = newValue;
        }

        private void OnSelectedPointChanged(ControlPoint oldValue, ControlPoint newValue)
        {
            selectedPoint = newValue;
            if (newValue != null)
            {
                SelectedAnnotation = newValue.Annotation;
            }
            else
            {
                Operations.Current = OperationType.None;
            }
        }

        private void OnHoverPointChanged(ControlPoint oldValue, ControlPoint newValue)
        {
            hoverPoint = newValue;
            if (newValue != null)
            {
                Cursor = CursorType.Cross;
            }
            else
            {
                Cursor = CursorType.Arrow;
            }
        }

        private void OnShowCharBoundChanged(bool oldValue, bool newValue)
        {
            showCharBound = newValue;
            InvalidatePaint();
        }

        private void OnIsReadOnlyChanged(bool oldValue, bool newValue)
        {
            isReadOnly = newValue;
            InvalidatePaint();
        }

        protected override void OnVerticalValueChanged(double oldValue, double newValue)
        {
            base.verticalValue = newValue;
            state.UpdateCurrentMatrix();
            base.OnVerticalValueChanged(oldValue, newValue);
            if (VerticalScrollAnimation == null)
            {
                Page = state.GetCenterPage();
            }
        }

        protected override void OnHorizontalValueChanged(double oldValue, double newValue)
        {
            base.horizontalValue = newValue;
            state.UpdateCurrentMatrix();
            base.OnHorizontalValueChanged(oldValue, newValue);
        }

#if __FORCE_GL__
        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
#else
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
#endif
        {
            if(e.Info.Width != Width
                || e.Info.Height != Height)
            OnSizeAllocated(e.Info.Width, e.Info.Height);
            state.XScaleFactor = (float)(e.Info.Width / Width);
            state.YScaleFactor = (float)(e.Info.Height / Height);
            base.OnPaintSurface(e);
        }

#if __FORCE_GL__
        protected override void OnPaintContent(SKPaintGLSurfaceEventArgs e)
#else
        protected override void OnPaintContent(SKPaintSurfaceEventArgs e)
#endif
        {
            if (Document == null)
                return;
            state.Draw(e.Surface.Canvas);
        }

        public override bool OnKeyDown(string keyName, KeyModifiers modifiers)
        {
            if (Operations.OnKeyDown(keyName, modifiers))
                return true;
            return base.OnKeyDown(keyName, modifiers);
        }

        private void OnTextSelectionChanged(object sender, EventArgs args)
        {
            InvalidatePaint();
        }

        private void OnOperationsChanged(object sender, EventArgs e)
        {
            //event is changed
        }

        protected override void OnTouch(TouchEventArgs e)
        {
            base.OnTouch(e);
            state.OnTouch(e.ActionType, e.MouseButton, e.Location);
        }

        public override bool OnScrolled(double delta)
        {
            if (KeyModifiers == KeyModifiers.None)
            {
                return base.OnScrolled(delta);
            }
            if (KeyModifiers == KeyModifiers.Ctrl)
            {
                state.Scale((float)delta);
            }
            return false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            state.UpdateCurrentMatrix((float)width, (float)height);
            ScrollTo(Page);
        }

        public void Reload()
        {
            var newDocument = Document.Reload(Operations);
            Operations.MoveToLast();
            Close();
            Document = newDocument;
            InvalidatePaint();
        }

        public void Load(string filePath)
        {
            Close();
            Document = PdfDocumentViewModel.LoadFrom(filePath);
        }

        public void Load(Stream stream)
        {
            Close();
            Document = PdfDocumentViewModel.LoadFrom(stream);
        }

        public void Close()
        {
            Operations.ClearAll();
            var document = Document;
            Document = null;
            document?.Dispose();
        }

        public void ScrollTo(PdfPage page) => ScrollTo(Document.GetPageView(page));

        public void ScrollTo(IPdfPageViewModel page)
        {
            var location = state.ScrollTo(page);
            AnimateScroll(Math.Max(location.Y, 0), Math.Max(location.X, 0));
        }

        public void ScrollTo(Annotation annotation)
        {
            var location = state.ScrollTo(annotation);
            AnimateScroll(Math.Max(location.Y, 0), Math.Max(location.X, 0));
        }
    }
}
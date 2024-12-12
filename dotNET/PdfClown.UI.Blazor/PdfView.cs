using Microsoft.AspNetCore.Components;
using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.UI.Operations;
using PdfClown.UI.Text;
using PdfClown.Util.Math;
using SkiaSharp.Views.Blazor;
using System.Runtime.Versioning;

namespace PdfClown.UI.Blazor
{
    [SupportedOSPlatform("browser")]
    public partial class PdfView : SKScrollView, IPdfView
    {
        private bool showCharBound;
        private PdfViewFitMode fitMode = PdfViewFitMode.PageSize;
        private bool showMarkup = true;
        private bool isReadOnly;

        public PdfView()
        {
            Envir.Init();

            TextSelection = new TextSelection();
            TextSelection.Changed += OnTextSelectionChanged;

            Operations = new EditorOperations(this);
            Operations.Changed += OnOperationsChanged;
            Operations.CurrentPageChanged += OnCurrentPageChanged;
            Operations.ScaleChanged += OnScaleChanged;

            scroll.VScrolled += OnVScrolled;
            scroll.HScrolled += OnHScrolled;
        }

        [Parameter]
        public PdfViewFitMode FitMode { get; set; }

        [Parameter]
        public EventCallback<PdfViewFitMode> FitModeChanged { get; set; }

        [Parameter]
        public float ScaleContent { get; set; }

        [Parameter]
        public EventCallback<float> ScaleContentChanged { get; set; }

        [Parameter]
        public bool ShowMarkup { get; set; }

        [Parameter]
        public EventCallback<bool> ShowMarkupChanged { get; set; }

        public bool ScrollByPointer { get; set; } = true;

        [Parameter]
        public bool IsReadOnly { get; set; }

        [Parameter]
        public EventCallback<bool> IsReadOnlyChanged { get; set; }

        [Parameter]
        public bool ShowCharBound { get; set; }

        [Parameter]
        public EventCallback<bool> ShowCharBoundChanged { get; set; }

        public IPdfDocumentViewModel? Document
        {
            get => Operations.Document;
            set
            {
                if (Operations.Document != value)
                {
                    Operations.Document = value;
                    OnDocumentChanged(value);
                }
            }
        }

        public PdfPage? PdfPage
        {
            get => Page?.GetPage(Operations.State);
            set => Page = Document?.GetPageView(value);
        }

        public IPdfPageViewModel? Page
        {
            get => Operations.CurrentPage;
            set => Operations.CurrentPage = value;
        }

        public TextSelection TextSelection { get; private set; }

        public EditorOperations Operations { get; private set; }

        [Parameter]
        public bool IsEdited { get; set; }

        [Parameter]
        public EventCallback<bool>? IsEditedChanged { get; set; }

        [Parameter]
        public int PagesCount { get; set; }

        [Parameter]
        public EventCallback<int> PagesCountChanged { get; set; }

        [Parameter]
        public int NewPageNumber { get; set; }

        [Parameter]
        public EventCallback<int> NewPageNumberChanged { get; set; }

        [Parameter]
        public int PageNumber { get; set; }

        [Parameter]
        public EventCallback<int> PageNumberChanged { get; set; }


        public event PdfDocumentEventHandler? DocumentChanged;

        public void NextPage() => NewPageNumber += 1;

        private bool CanNextPage() => PageNumber < PagesCount;

        public void PrevPage() => NewPageNumber -= 1;

        private bool CanPrevPage() => PageNumber > 1;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (fitMode != FitMode)
            {
                OnFitModeChanged(fitMode, FitMode);
            }
            if (Operations.Scale != ScaleContent)
            {
                Operations.SetScale(ScaleContent);
            }
            if (showMarkup != ShowMarkup)
            {
                OnShowMarkupChanged(showMarkup, ShowMarkup);
            }
            if (isReadOnly != IsReadOnly)
            {
                OnIsReadOnlyChanged(isReadOnly, IsReadOnly);
            }
            if (showCharBound != ShowCharBound)
            {
                OnShowCharBoundChanged(showCharBound, ShowCharBound);
            }
            //if (state.CurrentPageNumber != PageNumber)
            //{
            //    state.CurrentPageNumber = PageNumber;
            //}
            if (Operations.NewPageNumber != NewPageNumber)
            {
                Operations.NewPageNumber = NewPageNumber;
            }
        }

        private void OnFitModeChanged(PdfViewFitMode oldValue, PdfViewFitMode newValue)
        {
            fitMode = newValue;
            //InvalidateSurface();
            if (Page != null)
            {
                ScrollTo(Page);
            }
        }

        private void OnShowMarkupChanged(bool oldValue, bool newValue)
        {
            showMarkup = newValue;
            InvalidatePaint();
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

        private void OnVScrolled(object? sender, ScrollEventArgs e)
        {
            Operations.UpdateNavigationMatrix();
            if (!IsVScrollAnimation)
            {
                Page = Operations.GetCenterPage();
            }
        }

        private void OnHScrolled(object? sender, ScrollEventArgs e)
        {
            Operations.UpdateNavigationMatrix();
        }
        
#if __FORCE_GL__
        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
#else
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
#endif
        {
            Operations.State.SetWindowScale((float)(e.Info.Width / Width), (float)(e.Info.Height / Height));
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
            Operations.Draw(e.Surface.Canvas);
        }

        public override bool OnKeyDown(string keyName, KeyModifiers modifiers)
        {
            if (Operations.OnKeyDown(keyName, modifiers))
                return true;
            return base.OnKeyDown(keyName, modifiers);
        }

        private void OnDocumentChanged(IPdfDocumentViewModel? value)
        {
            _ = PagesCountChanged.InvokeAsync(Operations.PagesCount);
            DocumentChanged?.Invoke(new PdfDocumentEventArgs(value));
        }

        private void OnCurrentPageChanged(PdfPageEventArgs e)
        {
            if (Operations.CurrentPageNumber != PageNumber)
                _ = PageNumberChanged.InvokeAsync(Operations.CurrentPageNumber);
            if (Operations.NewPageNumber != NewPageNumber)
                _ = NewPageNumberChanged.InvokeAsync(Operations.NewPageNumber);
        }

        private void OnScaleChanged(FloatEventArgs e)
        {
            if (e.Value != ScaleContent)
                _ = ScaleContentChanged.InvokeAsync(e.Value);
        }


        private void OnTextSelectionChanged(TextSelectionEventArgs args)
        {
            InvalidatePaint();
        }

        private void OnOperationsChanged(object? sender, EventArgs? e)
        {
            IsEditedChanged?.InvokeAsync(Operations.HashOperations);
        }

        protected override void OnTouch(TouchEventArgs e)
        {
            base.OnTouch(e);
            if (e.Handled)
                return;
            Operations.OnTouch(e.ActionType, e.MouseButton, e.Location.Scale(scroll.XScaleFactor, scroll.YScaleFactor));
        }

        public override bool OnScrolled(double delta)
        {
            if (KeyModifiers == KeyModifiers.None)
            {
                return base.OnScrolled(delta);
            }
            if (KeyModifiers == KeyModifiers.Ctrl)
            {
                Operations.ScaleToPointer((float)delta);
            }
            return false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            Operations.UpdateNavigationMatrix((float)width, (float)height);
            if (Page != null)
            {
                ScrollTo(Page);
            }
        }

        public void Reload()
        {
            var newDocument = Document?.Reload(Operations);
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
            Operations.ClearOperations();
            TextSelection.Clear();
            var document = Document;
            Document = null;
            document?.Dispose();
        }

        public void ScrollTo(PdfPage page)
        {
            if (Document?.GetPageView(page) is IPdfPageViewModel pageView)
                ScrollTo(pageView);
        }

        public void ScrollTo(IPdfPageViewModel page)
        {
            var location = Operations.ScrollTo(page);
            AnimateScroll(Math.Max(location.Y, 0), Math.Max(location.X, 0));
        }

        public void ScrollTo(Annotation annotation)
        {
            var location = Operations.ScrollTo(annotation);
            AnimateScroll(Math.Max(location.Y, 0), Math.Max(location.X, 0));
        }
    }
}
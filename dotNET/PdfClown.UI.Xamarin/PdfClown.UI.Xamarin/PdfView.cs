using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.UI.Operations;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace PdfClown.UI
{

    public partial class PdfView : SKScrollView, IPdfView
    {
        public static readonly BindableProperty FitModeProperty = BindableProperty.Create(nameof(FitMode), typeof(PdfViewFitMode), typeof(PdfView), PdfViewFitMode.PageSize,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnFitModeChanged((PdfViewFitMode)oldValue, (PdfViewFitMode)newValue));
        public static readonly BindableProperty PageBackgroundProperty = BindableProperty.Create(nameof(PageBackground), typeof(Color), typeof(PdfView), Color.White,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnPageBackgroundChanged((Color)oldValue, (Color)newValue));
        public static readonly BindableProperty ScaleContentProperty = BindableProperty.Create(nameof(ScaleContent), typeof(float), typeof(PdfView), 1F,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnScaleContentChanged((float)oldValue, (float)newValue));
        public static readonly BindableProperty ShowMarkupProperty = BindableProperty.Create(nameof(ShowMarkup), typeof(bool), typeof(PdfView), true,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnShowMarkupChanged((bool)oldValue, (bool)newValue));
        public static readonly BindableProperty HoverAnnotationProperty = BindableProperty.Create(nameof(HoverAnnotation), typeof(Annotation), typeof(PdfView), null,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnHoverAnnotationChanged((Annotation)oldValue, (Annotation)newValue));
        public static readonly BindableProperty SelectedAnnotationProperty = BindableProperty.Create(nameof(SelectedAnnotation), typeof(Annotation), typeof(PdfView), null,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnSelectedAnnotationChanged((Annotation)oldValue, (Annotation)newValue));
        public static readonly BindableProperty SelectedMarkupProperty = BindableProperty.Create(nameof(SelectedMarkup), typeof(Markup), typeof(PdfView), null,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnSelectedMarkupChanged((Markup)oldValue, (Markup)newValue));
        public static readonly BindableProperty SelectedPointProperty = BindableProperty.Create(nameof(SelectedPoint), typeof(ControlPoint), typeof(PdfView), null,
            propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnSelectedPointChanged((ControlPoint)oldValue, (ControlPoint)newValue));
        public static readonly BindableProperty HoverPointProperty = BindableProperty.Create(nameof(HoverPoint), typeof(ControlPoint), typeof(PdfView), null,
           propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnHoverPointChanged((ControlPoint)oldValue, (ControlPoint)newValue));
        public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(PdfView), false,
           propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnIsReadOnlyChanged((bool)oldValue, (bool)newValue));
        public static readonly BindableProperty ShowCharBoundProperty = BindableProperty.Create(nameof(ShowCharBound), typeof(bool), typeof(PdfView), false,
           propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnShowCharBoundChanged((bool)oldValue, (bool)newValue));

        internal readonly SKPaint paintPageBackground = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.White };

        private float oldScale = 1;
        private float scale = 1;

        private readonly PdfViewState state = new PdfViewState();

        private Annotation selectedAnnotation;
        private ControlPoint selectedPoint;

        private bool readOnly;
        private bool showCharBound;
        private IPdfDocumentViewModel document;
        private IPdfPageViewModel currentPage;

        public PdfView()
        {
            Envir.Init();
            state.Viewer = this;

            TextSelection = new TextSelection();

            Operations = new EditOperationList { Viewer = this };
            Operations.Changed += OnOperationsChanged;

            UndoCommand = new Command(() => Operations.Undo(), () => Operations.CanUndo);
            RedoCommand = new Command(() => Operations.Redo(), () => Operations.CanRedo);
            PrevPageCommand = new Command(() => PrevPage(), CanPrevPage);
            NextPageCommand = new Command(() => NextPage(), CanNextPage);

            TextSelection.Changed += OnTextSelectionChanged;
        }

        public PdfViewFitMode FitMode
        {
            get => (PdfViewFitMode)GetValue(FitModeProperty);
            set => SetValue(FitModeProperty, value);
        }

        public Color PageBackground
        {
            get => (Color)GetValue(PageBackgroundProperty);
            set => SetValue(PageBackgroundProperty, value);
        }

        public float ScaleContent
        {
            get => (float)GetValue(ScaleContentProperty);
            set => SetValue(ScaleContentProperty, value);
        }

        public bool ShowMarkup
        {
            get => (bool)GetValue(ShowMarkupProperty);
            set => SetValue(ShowMarkupProperty, value);
        }

        public Annotation SelectedAnnotation
        {
            get => (Annotation)GetValue(SelectedAnnotationProperty);
            set => SetValue(SelectedAnnotationProperty, value);
        }

        public Annotation HoverAnnotation
        {
            get => (Annotation)GetValue(HoverAnnotationProperty);
            set => SetValue(HoverAnnotationProperty, value);
        }

        public Markup SelectedMarkup
        {
            get => (Markup)GetValue(SelectedMarkupProperty);
            set => SetValue(SelectedMarkupProperty, value);
        }

        public ControlPoint SelectedPoint
        {
            get => (ControlPoint)GetValue(SelectedPointProperty);
            set => SetValue(SelectedPointProperty, value);
        }

        public ControlPoint HoverPoint
        {
            get => (ControlPoint)GetValue(HoverPointProperty);
            set => SetValue(HoverPointProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool ShowCharBound
        {
            get => (bool)GetValue(ShowCharBoundProperty);
            set => SetValue(ShowCharBoundProperty, value);
        }

        public IPdfDocumentViewModel Document
        {
            get => document;
            set
            {
                if (document == value)
                    return;
                Operations.DocumentView = value;

                SelectedAnnotation = null;
                SelectedPoint = null;
                document = value;
                OnPropertyChanged(nameof(PagesCount));
                UpdateMaximums();

                if (document != null)
                {
                    Page = document.PageViews.FirstOrDefault();
                    ScrollTo(Page);
                }
                DocumentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public SKSize DocumentSize => Document?.Size ?? SKSize.Empty;

        public PdfPage CurrentPage
        {
            get => Page?.GetPage(state);
            set => Page = Document.GetPageView(value);
        }

        public IPdfPageViewModel Page
        {
            get
            {
                if (currentPage == null
                    || currentPage.Document != Document)
                {
                    currentPage = state.GetCenterPage();
                }
                return currentPage;
            }
            set
            {
                if (currentPage != value)
                {
                    currentPage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentPage));
                    OnPropertyChanged(nameof(PageNumber));
                    OnPropertyChanged(nameof(PageNumberWithScroll));
                    ((Command)NextPageCommand).ChangeCanExecute();
                    ((Command)PrevPageCommand).ChangeCanExecute();
                }
            }
        }

        public TextSelection TextSelection { get; private set; }

        public EditOperationList Operations { get; private set; }

        public bool IsChanged => Operations.HashOperations;

        public int PagesCount
        {
            get => Document?.PagesCount ?? 0;
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

        public ICommand NextPageCommand { get; set; }

        public ICommand PrevPageCommand { get; set; }

        public ICommand RedoCommand { get; set; }

        public ICommand UndoCommand { get; set; }

        public event EventHandler<AnnotationEventArgs> SelectedAnnotationChanged;

        public event EventHandler<EventArgs> DocumentChanged;

        public void NextPage() => PageNumberWithScroll += 1;

        private bool CanNextPage() => PageNumber < PagesCount;

        public void PrevPage() => PageNumberWithScroll -= 1;

        private bool CanPrevPage() => PageNumber > 1;

        private void OnFitModeChanged(PdfViewFitMode oldValue, PdfViewFitMode newValue)
        {
            ScrollTo(Page);
        }

        private void OnPageBackgroundChanged(Color oldValue, Color newValue)
        {
            paintPageBackground.Color = newValue.ToSKColor();
        }

        private void OnScaleContentChanged(float oldValue, float newValue)
        {
            oldScale = oldValue;
            scale = newValue;
            UpdateMaximums();
        }

        private void OnShowMarkupChanged(bool oldValue, bool newValue)
        {
            InvalidateSurface();
        }

        private void OnHoverAnnotationChanged(Annotation oldValue, Annotation newValue)
        {
        }

        private void OnSelectedAnnotationChanged(Annotation oldValue, Annotation newValue)
        {
            selectedAnnotation = newValue;
            SelectedMarkup = newValue as Markup;
            Operations.Annotation = newValue;
            SelectedAnnotationChanged?.Invoke(this, new AnnotationEventArgs(newValue));
            InvalidateSurface();
        }

        private void OnSelectedMarkupChanged(Markup oldValue, Markup newValue)
        {
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
            InvalidateSurface();
        }

        private void OnIsReadOnlyChanged(bool oldValue, bool newValue)
        {
            readOnly = newValue;
        }

        protected override void OnVerticalValueChanged(double oldValue, double newValue)
        {
            UpdateCurrentMatrix();
            base.OnVerticalValueChanged(oldValue, newValue);
            if (ScrollAnimation == null)
            {
                Page = state.GetCenterPage();
            }
        }

        protected override void OnHorizontalValueChanged(double oldValue, double newValue)
        {
            UpdateCurrentMatrix();
            base.OnHorizontalValueChanged(oldValue, newValue);
        }

        protected override void OnWindowScaleChanged()
        {
            base.OnWindowScaleChanged();
            state.WindowScaleMatrix = SKMatrix.CreateScale(XScaleFactor, YScaleFactor);
        }

        protected override void OnPaintContent(SKPaintSurfaceEventArgs e)
        {
            if (Document == null)
                return;
            state.Draw(e.Surface.Canvas);
        }

        public override bool OnKeyDown(string keyName, KeyModifiers modifiers)
        {
            if (string.Equals(keyName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                if (!readOnly)
                {
                    if (selectedPoint is IndexControlPoint indexControlPoint)
                    {
                        Operations.BeginOperation(selectedPoint.Annotation, OperationType.PointRemove, indexControlPoint, indexControlPoint.MappedPoint, indexControlPoint.MappedPoint);
                        ((VertexShape)selectedPoint.Annotation).RemovePoint(indexControlPoint.Index);
                        return true;
                    }
                    else if (selectedAnnotation != null)
                    {
                        Operations.RemoveAnnotation(selectedAnnotation);
                    }
                }
            }
            else if (string.Equals(keyName, "Escape", StringComparison.OrdinalIgnoreCase))
            {
                if (selectedPoint != null
                    && selectedAnnotation is VertexShape vertexShape
                    && Operations.Current == OperationType.PointAdd)
                {
                    Operations.CloseVertextShape(vertexShape);
                }
            }
            else if (string.Equals(keyName, "Z", StringComparison.OrdinalIgnoreCase))
            {
                if (modifiers == KeyModifiers.Ctrl)
                {
                    Operations.Undo();
                    return true;
                }
                else if (modifiers == (KeyModifiers.Ctrl | KeyModifiers.Shift))
                {
                    Operations.Redo();
                    return true;
                }
            }
            return base.OnKeyDown(keyName, modifiers);
        }

        private void OnTextSelectionChanged(object sender, EventArgs args)
        {
            InvalidateSurface();
        }

        private void OnOperationsChanged(object sender, EventArgs e)
        {
            //OnPropertyChanged(nameof(CanUndo));
            //OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(IsChanged));
            ((Command)UndoCommand).ChangeCanExecute();
            ((Command)RedoCommand).ChangeCanExecute();
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            base.OnTouch(e);
            state.TouchAction = (TouchAction)(int)e.ActionType;
            state.TouchButton = (MouseButton)(int)e.MouseButton;
            state.PointerLocation = e.Location;

            if (e.MouseButton == SKMouseButton.Middle)
            {
                if (e.ActionType == SKTouchAction.Pressed)
                {
                    state.MoveLocation = state.PointerLocation;
                    Cursor = CursorType.ScrollAll;
                    return;
                }
                else if (e.ActionType == SKTouchAction.Moved)
                {
                    var vector = state.PointerLocation - state.MoveLocation;
                    HorizontalValue -= vector.X;
                    VerticalValue -= vector.Y;
                    state.MoveLocation = state.PointerLocation;
                    return;
                }
            }
            if (Document == null || !Document.IsPaintComplete)
            {
                return;
            }
            if (state.Touch())
            {
                return;
            }

            if (Operations.Current != OperationType.AnnotationDrag)
                Cursor = CursorType.Arrow;
            state.PageView = null;
        }

        public override void OnScrolled(int delta, KeyModifiers keyModifiers)
        {
            if (keyModifiers == KeyModifiers.None)
            {
                base.OnScrolled(delta, keyModifiers);
            }
            if (keyModifiers == KeyModifiers.Ctrl)
            {
                var scaleStep = 0.06F * Math.Sign(delta);

                var newSclae = scale + scaleStep + scaleStep * scale;
                if (newSclae < 0.01F)
                    newSclae = 0.01F;
                if (newSclae > 60F)
                    newSclae = 60F;
                if (newSclae != scale)
                {
                    var unscaleLocations = new SKPoint(state.PointerLocation.X / XScaleFactor, state.PointerLocation.Y / YScaleFactor);
                    var oldSpacePoint = state.InvertNavigationMatrix.MapPoint(unscaleLocations);

                    ScaleContent = newSclae;

                    var newCurrentLocation = state.NavigationMatrix.MapPoint(oldSpacePoint);

                    var vector = newCurrentLocation - unscaleLocations;
                    if (HorizontalScrollBarVisible)
                    {
                        HorizontalValue += vector.X;
                    }
                    if (VerticalScrollBarVisible)
                    {
                        VerticalValue += vector.Y;
                    }
                }
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdateCurrentMatrix();
            ScrollTo(Page);
        }

        public void Reload()
        {
            var newDocument = Document.Reload(Operations);
            Operations.MoveToLast();
            Close();
            Document = newDocument;
            InvalidateSurface();
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
            if (page == null || Document == null)
            {
                return;
            }
            ScrollAnimation = new Animation();
            if (FitMode == PdfViewFitMode.DocumentWidth)
            {
                ScaleContent = (float)Width / Document.Size.Width;
            }
            else if (FitMode == PdfViewFitMode.PageWidth)
            {
                ScaleContent = (float)Width / (page.Bounds.Width + 10);
            }
            else if (FitMode == PdfViewFitMode.PageSize)
            {
                var vScale = (float)Height / (page.Bounds.Height + 10);
                var hScale = (float)Width / (page.Bounds.Width + 10);
                ScaleContent = hScale < vScale ? hScale : vScale;
            }

            var matrix = SKMatrix.CreateScale(scale, scale);
            var bound = matrix.MapRect(page.Bounds);
            var top = bound.Top - (state.WindowArea.MidY - bound.Height / 2);
            var left = bound.Left - (state.WindowArea.MidX - bound.Width / 2);
            AnimateScroll(Math.Max(top, 0), Math.Max(left, 0));
        }

        public void ScrollTo(Annotation annotation)
        {
            if (annotation?.Page == null)
            {
                return;
            }

            var pageView = Document.GetPageView(annotation.Page);
            if (pageView == null)
            {
                return;
            }
            var matrix = SKMatrix.CreateScale(scale, scale)
                .PreConcat(pageView.Matrix)
                .PreConcat(pageView.Document.Matrix);
            var bound = annotation.GetViewBounds(matrix);
            var top = bound.Top - (state.WindowArea.MidY - bound.Height / 2);
            var left = bound.Left - (state.WindowArea.MidX - bound.Width / 2);
            AnimateScroll(Math.Max(top, 0), Math.Max(left, 0));
        }

        public void UpdateMaximums()
        {
            UpdateCurrentMatrix();
            HorizontalMaximum = DocumentSize.Width * scale;
            VerticalMaximum = DocumentSize.Height * scale;
            InvalidateSurface();
        }

        private void UpdateCurrentMatrix()
        {
            state.WindowArea = SKRect.Create(0, 0, (float)Width, (float)Height);

            var maximumWidth = DocumentSize.Width * scale;
            var maximumHeight = DocumentSize.Height * scale;
            var dx = 0F; var dy = 0F;
            if (maximumWidth < state.WindowArea.Width)
            {
                dx = (float)((Width - 10) - maximumWidth) / 2;
            }

            if (maximumHeight < state.WindowArea.Height)
            {
                dy = (float)(Height - maximumHeight) / 2;
            }
            state.NavigationMatrix = new SKMatrix(
                scale, 0, ((float)-HorizontalValue) + dx,
                0, scale, ((float)-VerticalValue) + dy,
                0, 0, 1);
        }
    }
}

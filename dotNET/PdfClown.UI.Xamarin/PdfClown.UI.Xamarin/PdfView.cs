﻿using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.UI.Operations;
using PdfClown.UI.Text;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.IO;
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
        public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(PdfView), false,
           propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnIsReadOnlyChanged((bool)oldValue, (bool)newValue));
        public static readonly BindableProperty ShowCharBoundProperty = BindableProperty.Create(nameof(ShowCharBound), typeof(bool), typeof(PdfView), false,
           propertyChanged: (bindable, oldValue, newValue) => ((PdfView)bindable).OnShowCharBoundChanged((bool)oldValue, (bool)newValue));

        internal readonly SKPaint paintPageBackground = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.White };

        private readonly PdfViewState state;

        private bool showCharBound;

        public PdfView()
        {
            state = new PdfViewState { Viewer = this };
            state.CurrentPageChanged += OnCurrentPageChanged;
            state.ScaleChanged += OnScaleChanged;

            TextSelection = new TextSelection();
            TextSelection.Changed += OnTextSelectionChanged;

            Operations = new EditOperationList { Viewer = this };
            Operations.Changed += OnOperationsChanged;

            UndoCommand = new Command(() => Operations.Undo(), () => Operations.CanUndo);
            RedoCommand = new Command(() => Operations.Redo(), () => Operations.CanRedo);
            PrevPageCommand = new Command(() => PrevPage(), CanPrevPage);
            NextPageCommand = new Command(() => NextPage(), CanNextPage);
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

        public bool ScrollByPointer { get; set; }

        public bool ShowMarkup
        {
            get => (bool)GetValue(ShowMarkupProperty);
            set => SetValue(ShowMarkupProperty, value);
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
            get => state.Document;
            set
            {
                if (state.Document != value)
                {
                    state.Document = value;
                    DocumentChanged?.Invoke(new PdfDocumentEventArgs(value));
                    OnPropertyChanged(nameof(PagesCount));
                }
            }
        }

        public SKSize DocumentSize => Document?.Size ?? SKSize.Empty;

        public PdfPage PdfPage
        {
            get => Page?.GetPage(state);
            set => Page = Document.GetPageView(value);
        }

        public IPdfPageViewModel Page
        {
            get => state.CurrentPage;
            set => state.CurrentPage = value;
        }

        public TextSelection TextSelection { get; private set; }

        public EditOperationList Operations { get; private set; }

        public bool IsChanged => Operations.HashOperations;

        public int PagesCount
        {
            get => state.PagesCount;
        }

        public int NewPageNumber
        {
            get => state.NewPageNumber;
            set => state.NewPageNumber = value;
        }

        public int PageNumber
        {
            get => state.CurrentPageNumber;
            set => state.CurrentPageNumber = value;
        }

        public ICommand NextPageCommand { get; set; }

        public ICommand PrevPageCommand { get; set; }

        public ICommand RedoCommand { get; set; }

        public ICommand UndoCommand { get; set; }

        public event PdfDocumentEventHandler DocumentChanged;

        public void NextPage() => NewPageNumber += 1;

        private bool CanNextPage() => PageNumber < PagesCount;

        public void PrevPage() => NewPageNumber -= 1;

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
            state.Scale = newValue;
        }

        private void OnShowMarkupChanged(bool oldValue, bool newValue)
        {
            InvalidatePaint();
        }

        private void OnShowCharBoundChanged(bool oldValue, bool newValue)
        {
            showCharBound = newValue;
            InvalidatePaint();
        }

        private void OnIsReadOnlyChanged(bool oldValue, bool newValue)
        {
        }

        protected override void OnVerticalValueChanged(double oldValue, double newValue)
        {
            state.UpdateCurrentMatrix();
            base.OnVerticalValueChanged(oldValue, newValue);
            if (VerticalScrollAnimation == null)
            {
                Page = state.GetCenterPage();
            }
        }

        protected override void OnHorizontalValueChanged(double oldValue, double newValue)
        {
            state.UpdateCurrentMatrix();
            base.OnHorizontalValueChanged(oldValue, newValue);
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            state.XScaleFactor = (float)(e.Info.Width / Width);
            state.YScaleFactor = (float)(e.Info.Height / Height);
            base.OnPaintSurface(e);
        }

        protected override void OnPaintContent(SKPaintSurfaceEventArgs e)
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

        private void OnScaleChanged(FloatEventArgs e)
        {
            ScaleContent = e.Value;
        }

        private void OnCurrentPageChanged(PdfPageEventArgs e)
        {
            OnPropertyChanged(nameof(Page));
            OnPropertyChanged(nameof(PdfPage));
            OnPropertyChanged(nameof(PageNumber));
            OnPropertyChanged(nameof(NewPageNumber));
            ((Command)NextPageCommand).ChangeCanExecute();
            ((Command)PrevPageCommand).ChangeCanExecute();
        }

        private void OnTextSelectionChanged(TextSelectionEventArgs args)
        {
            InvalidatePaint();
        }

        private void OnOperationsChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsChanged));
            ((Command)UndoCommand).ChangeCanExecute();
            ((Command)RedoCommand).ChangeCanExecute();
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            base.OnTouch(new SKTouchEventArgs(e.Id, e.ActionType, e.MouseButton, e.DeviceType,
                new SKPoint(e.Location.X / state.XScaleFactor, e.Location.Y / state.YScaleFactor),
                e.InContact));
            if (e.Handled)
                return;
            state.OnTouch((TouchAction)(int)e.ActionType, (MouseButton)(int)e.MouseButton, e.Location);
        }

        public override bool OnScrolled(int delta)
        {
            if (KeyModifiers == KeyModifiers.None)
            {
                return base.OnScrolled(delta);
            }
            if (KeyModifiers == KeyModifiers.Ctrl)
            {
                state.ScaleToPointer(delta);
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
            Operations.ClearOperations();
            TextSelection.Clear();
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

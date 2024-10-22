using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.UI.Text;
using PdfClown.UI.Operations;
using System;

namespace PdfClown.UI
{
    public interface IPdfView
    {
        IPdfDocumentViewModel Document { get; set; }
        IPdfPageViewModel Page { get; set; }
        PdfPage CurrentPage { get; set; }
        CursorType Cursor { get; set; }
        bool ShowMarkup { get; }
        bool ShowCharBound { get; }
        bool ScrollByPointer { get; set; }
        EditOperationList Operations { get; }
        bool IsReadOnly { get; set; }
        int PagesCount { get; set; }
        int PageNumber { get; set; }

        TextSelection TextSelection { get; }

        double Width { get; }
        double Height { get; }

        double HorizontalMaximum { get; set; }
        double VerticalMaximum { get; set; }
        double HorizontalValue { get; set; }
        double VerticalValue { get; set; }
        PdfViewFitMode FitMode { get; set; }
        float ScaleContent { get; set; }

        event PdfDocumentEventHandler DocumentChanged;

        void InvalidatePaint();

        void ScrollTo(Annotation annotation);
        void ScrollTo(PdfPage page);
        void ScrollTo(IPdfPageViewModel currentPageView);
        void Reload();
    }
}

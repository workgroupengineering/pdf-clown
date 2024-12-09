using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.UI.Text;
using PdfClown.UI.Operations;

namespace PdfClown.UI
{
    public interface IPdfView: ISKScrollView
    {
        IPdfDocumentViewModel Document { get; set; }
        IPdfPageViewModel Page { get; set; }
        PdfPage PdfPage { get; set; }
        CursorType Cursor { get; set; }
        bool ShowMarkup { get; }
        bool ShowCharBound { get; }
        bool ScrollByPointer { get; set; }
        EditOperationList Operations { get; }
        bool IsReadOnly { get; set; }
        int PagesCount { get; }
        int PageNumber { get; set; }
        int NewPageNumber { get; set; }

        TextSelection TextSelection { get; }

        double Width { get; }
        double Height { get; }

        PdfViewFitMode FitMode { get; set; }
        float ScaleContent { get; set; }

        event PdfDocumentEventHandler DocumentChanged;

        void ScrollTo(Annotation annotation);
        void ScrollTo(PdfPage page);
        void ScrollTo(IPdfPageViewModel currentPageView);
        void Reload();
    }
}

using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.UI.Text;
using PdfClown.UI.Operations;

namespace PdfClown.UI
{
    public interface IPdfView: ISKScrollView
    {
        IPdfDocumentViewModel? Document { get; set; }
        IPdfPageViewModel? Page { get; set; }
        PdfPage? PdfPage { get; set; }
        CursorType Cursor { get; set; }
        bool ShowMarkup { get; set; }
        bool ShowCharBound { get; set; }
        bool ScrollByPointer { get; set; }
        EditorOperations Operations { get; }
        bool IsReadOnly { get; set; }
        bool IsModified { get; }
        int PagesCount { get; }
        int PageNumber { get; set; }
        int NewPageNumber { get; set; }

        TextSelection TextSelection { get; }

        PdfViewFitMode FitMode { get; set; }
        float ScaleContent { get; set; }

        void ScrollTo(Annotation annotation);
        void ScrollTo(PdfPage page);
        void ScrollTo(IPdfPageViewModel page);
        void Reload();
    }
}

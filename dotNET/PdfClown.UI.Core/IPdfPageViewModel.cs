using PdfClown.Documents;
using PdfClown.Documents.Contents;
using PdfClown.Documents.Interaction.Annotations;
using SkiaSharp;
using System.Collections.Generic;

namespace PdfClown.UI
{
    public interface IPdfPageViewModel : IBoundHandler
    {
        int Index { get; }
        SKMatrix Matrix { get; set; }
        int Order { get; }
        PdfPage GetPage(PdfViewState state);
        IPdfDocumentViewModel Document { get; set; }

        bool Draw(PdfViewState state);
        void Touch(PdfViewState state);
        Annotation GetAnnotation(string name);
        IEnumerable<Annotation> GetAnnotations();
        IEnumerable<ITextString> GetStrings();
    }
}
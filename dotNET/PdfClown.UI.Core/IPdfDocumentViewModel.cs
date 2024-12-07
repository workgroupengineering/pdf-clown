using PdfClown.Documents;
using PdfClown.Documents.Interaction.Forms;
using PdfClown.UI.Operations;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PdfClown.UI
{
    public interface IPdfDocumentViewModel : IDisposable, IBoundHandler
    {
        float AvgHeigth { get; }
        bool IsClosed { get; }
        bool IsPaintComplete { get; }
        IEnumerable<IPdfPageViewModel> PageViews { get; }
        SKSize Size { get; }
        int PagesCount { get; }
        IEnumerable<Field> Fields { get; }
        IPdfPageViewModel this[int inde] { get; }

        event PdfAnnotationEventHandler AnnotationAdded;
        event PdfAnnotationEventHandler AnnotationRemoved;

        PdfDocumentViewModel GetDocumentView(PdfDocument document);
        PdfPageViewModel GetPageView(PdfPage page);
        IPdfDocumentViewModel Reload(EditOperationList operations);
        bool ContainsField(string fieldName);
    }
}
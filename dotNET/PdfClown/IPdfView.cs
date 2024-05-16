using PdfClown.Documents;
using PdfClown.Documents.Interaction.Annotations;
using System;
using System.Collections.Generic;

namespace PdfClown
{
    public interface IPdfView
    {
        PdfDocument Document { get; }
        bool IsReadOnly { get; set; }        
        int PagesCount { get; }
        int PageNumber { get; set; }
        PdfPage CurrentPage { get; set; }

        Annotation SelectedAnnotation { get; set; }

        event EventHandler<AnnotationEventArgs> AnnotationAdded;
        event EventHandler<AnnotationEventArgs> AnnotationRemoved;
        event EventHandler<AnnotationEventArgs> SelectedAnnotationChanged;

        IEnumerable<Annotation> RemoveAnnotation(Annotation annotation);
        void InvalidateSurface();

        bool Redo();
        bool Undo();

        void ClearOperations();
        void RejectOperations();
        void ScrollTo(Annotation annotation);
        void ScrollTo(PdfPage page);
    }
}
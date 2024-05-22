using PdfClown.Documents;
using PdfClown.Documents.Contents;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Util.Math.Geom;
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
        List<TextChar> TextSelection { get; }
        Quad? TextSelectionQuad { get; }
        string TextSelectionString { get; }

        event EventHandler<AnnotationEventArgs> AnnotationAdded;
        event EventHandler<AnnotationEventArgs> AnnotationRemoved;
        event EventHandler<AnnotationEventArgs> SelectedAnnotationChanged;
        event EventHandler<EventArgs> TextSelectionChanged;
        event EventHandler<EventArgs> DocumentChanged;

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
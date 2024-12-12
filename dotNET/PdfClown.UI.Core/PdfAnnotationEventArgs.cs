using PdfClown.Documents.Interaction.Annotations;
using System;
using System.ComponentModel;

namespace PdfClown.UI
{
    public delegate void PdfAnnotationEventHandler(PdfAnnotationEventArgs e);
    public class PdfAnnotationEventArgs : CancelEventArgs
    {
        public PdfAnnotationEventArgs(Annotation? annotation) : base(false)
        {
            this.Annotation = annotation;
        }

        public Annotation? Annotation { get; }
    }
}
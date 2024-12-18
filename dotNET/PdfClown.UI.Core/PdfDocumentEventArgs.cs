using System;

namespace PdfClown.UI
{
    public delegate void PdfDocumentEventHandler(PdfDocumentEventArgs e);

    public class PdfDocumentEventArgs : EventArgs
    {
        public IPdfDocumentViewModel? Document { get; }

        public PdfDocumentEventArgs(IPdfDocumentViewModel? document)
        {
            Document = document;
        }
    }
}

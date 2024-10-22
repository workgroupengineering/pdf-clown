using System;

namespace PdfClown.UI.Text
{
    public delegate void TextSelectionEventHandler(TextSelectionEventArgs args);

    public class TextSelectionEventArgs : EventArgs
    {
        public TextSelectionEventArgs(TextSelection textSelection)
        {
            TextSelection = textSelection;
        }

        public TextSelection TextSelection { get; }
    }
}
using PdfClown.Documents.Contents;
using PdfClown.Util.Math.Geom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfClown.UI
{
    public class TextSelection
    {
        private string selectedString;
        private Quad? textSelectedQuad;
        private TextChar startChar;

        public List<TextChar> Chars { get; private set; } = new List<TextChar>();

        public string String
        {
            get
            {
                if (selectedString == null)
                {
                    var textBuilder = new StringBuilder();
                    TextChar prevTextChar = null;
                    foreach (TextChar textChar in Chars)
                    {
                        if (prevTextChar != null && prevTextChar.TextString != textChar.TextString)
                        {
                            textBuilder.Append(' ');
                        }
                        textBuilder.Append(textChar.Value);
                        prevTextChar = textChar;
                    }
                    selectedString = textBuilder.ToString();
                }
                return selectedString;
            }
        }

        public Quad? Quad
        {
            get
            {
                if (textSelectedQuad == null)
                {
                    var result = new Quad();
                    foreach (TextChar textChar in Chars)
                    {
                        if (textChar.Quad.IsEmpty)
                            continue;
                        if (result.IsEmpty)
                        { result = textChar.Quad; }
                        else
                        { result.Union(textChar.Quad); }
                    }
                    textSelectedQuad = result;
                }
                return textSelectedQuad;
            }
        }

        public TextChar StartChar
        {
            get => startChar;
            set
            {
                if (startChar != value)
                {
                    startChar = value;
                    if (value != null)
                    {
                        ClearQuite();
                        if (value != null)
                        {
                            Chars.Add(value);
                        }
                    }
                    OnChanged();
                }
            }
        }

        public event EventHandler Changed;

        public bool Any() => Chars.Count > 0;

        public void Reset()
        {
            textSelectedQuad = null;
            selectedString = null;
        }

        public void Clear()
        {
            ClearQuite();
            OnChanged();
        }

        private void ClearQuite()
        {
            Reset();
            Chars.Clear();
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void AddRange(IEnumerable<TextChar> chars)
        {
            Chars.AddRange(chars);
            OnChanged();
        }

        public TextChar First() => Chars.First();
    }
}
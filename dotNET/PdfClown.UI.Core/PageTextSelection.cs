using PdfClown.Documents.Contents.Scanner;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PdfClown.UI
{
    public class PageTextSelection : IDisposable
    {
        private SKPath path;

        public List<TextChar> Chars { get; set; } = new List<TextChar>();

        public void Clear()
        {
            Chars.Clear();
            path?.Reset();
        }

        public void Dispose()
        {
            Clear();
            path?.Dispose();
        }

        public SKPath GetPath()
        {
            path ??= new SKPath();
            if (!path.IsEmpty)
                return path;
            foreach (var textChar in Chars)
            {
                path.AddPoly(textChar.Quad.GetPoints());
            }
            return path;
        }

        public void AddRange(IEnumerable<TextChar> chars)
        {
            Chars.AddRange(chars);
        }
    }
}
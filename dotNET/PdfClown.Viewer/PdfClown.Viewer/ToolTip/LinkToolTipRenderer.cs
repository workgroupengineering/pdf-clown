using Org.BouncyCastle.Asn1.X509;
using PdfClown.Documents.Interaction;
using PdfClown.Documents.Interaction.Annotations;
using SkiaSharp;
using System.Collections.Generic;

namespace PdfClown.Viewer.ToolTip
{
    public class LinkToolTipRenderer : AnnotationToolTipRenderer
    {
        public LinkToolTipRenderer()
            : base()
        {
        }

        public Link Link { get; set; }

        public override SKRect Measure()
        {
            ClearLines();

            var target = Link.Target is ITextDisplayable texted ? texted.GetDisplayName(): string.Empty;
            if(string.IsNullOrEmpty(target))
                return SKRect.Empty;

            var text = MeasureLine(target, paintToolTipText);
            ContentLines = new List<LineOfText> { text };

            var contentBound = text.Bound;
            contentBound.Offset(Indent, Indent);
            contentBound.Inflate(Indent, Indent);

            return ContentBound = contentBound;
        }
    }

}

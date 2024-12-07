using PdfClown.Documents.Interaction;
using PdfClown.Documents.Interaction.Annotations;
using SkiaSharp;
using System.Collections.Generic;

namespace PdfClown.UI.ToolTip
{
    public class LinkToolTipRenderer : AnnotationToolTipRenderer
    {
        public LinkToolTipRenderer()
            : base()
        {
        }

        public Link Link
        {
            get => Annotation as Link;
            set => Annotation = value;
        }

        public override SKRect Measure()
        {
            Clear();

            var target = Link.Target is ITextDisplayable texted ? texted.GetDisplayName() : string.Empty;
            if (string.IsNullOrEmpty(target))
                return SKRect.Empty;

            var text = MeasureLine(target, DefaultSKStyles.PaintToolTipText, DefaultSKStyles.FontToolTipText);
            ContentLines = new List<LineOfText> { text };

            var contentBound = text.Bound;
            contentBound.Offset(Indent, Indent);
            contentBound.Inflate(Indent, Indent);

            return ContentBound = contentBound;
        }
    }

}

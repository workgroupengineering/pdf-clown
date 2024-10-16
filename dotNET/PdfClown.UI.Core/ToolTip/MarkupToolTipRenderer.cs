using PdfClown.Documents.Interaction.Annotations;
using SkiaSharp;

namespace PdfClown.UI.ToolTip
{
    public class MarkupToolTipRenderer : AnnotationToolTipRenderer
    {
        public MarkupToolTipRenderer()
            : base()
        {
        }

        public Markup Markup { get => Annotation as Markup; set => Annotation = value; }

        public override SKRect Measure()
        {
            Clear();

            var athorText = Markup.Author ?? "User";
            var author = MeasureLine(athorText, DefaultSKStyles.PaintToolTipHeadText);

            var contentBound = author.Bound;
            ContentLines = MeasureText(Markup.Contents ?? "<empty>", DefaultSKStyles.PaintToolTipText, ref contentBound);
            ContentLines.Insert(0, author);
            contentBound.Offset(Indent, Indent);
            contentBound.Inflate(Indent, Indent);

            return ContentBound = contentBound;
        } 
        
    }

}

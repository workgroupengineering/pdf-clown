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

        public Markup Markup { get; set; }

        public override SKRect Measure()
        {
            ClearLines();

            var athorText = Markup.Author ?? "User";
            var author = MeasureLine(athorText, paintToolTipHeadText);

            var contentBound = author.Bound;
            ContentLines = MeasureText(Markup.Contents ?? "<empty>", paintToolTipText, ref contentBound);
            ContentLines.Insert(0, author);
            contentBound.Offset(Indent, Indent);
            contentBound.Inflate(Indent, Indent);

            return ContentBound = contentBound;
        }       
    }

}

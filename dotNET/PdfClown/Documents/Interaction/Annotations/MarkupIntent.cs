using PdfClown.Objects;
using PdfClown.Util;

namespace PdfClown.Documents.Interaction.Annotations
{
    public enum MarkupIntent
    {
        Text,

        FreeText,
        FreeTextCallout,
        FreeTextTypeWriter,

        Line,
        LineArrow,
        LineDimension,

        Polygon,
        PolygonCloud,
        PolygonDimension,

        PolyLine,
        PolyLineDimension
    }

    internal static class MarkupIntentExtension
    {
        private static readonly BiDictionary<MarkupIntent, string> codes;

        static MarkupIntentExtension()
        {
            codes = new BiDictionary<MarkupIntent, string>
            {
                [MarkupIntent.Text] = PdfName.Text.StringValue,
                [MarkupIntent.FreeText] = PdfName.FreeText.StringValue,
                [MarkupIntent.FreeTextCallout] = PdfName.FreeTextCallout.StringValue,
                [MarkupIntent.FreeTextTypeWriter] = PdfName.FreeTextTypeWriter.StringValue,

                [MarkupIntent.Line] = PdfName.Line.StringValue,
                [MarkupIntent.LineArrow] = PdfName.LineArrow.StringValue,
                [MarkupIntent.LineDimension] = PdfName.LineDimension.StringValue,

                [MarkupIntent.Polygon] = PdfName.Polygon.StringValue,
                [MarkupIntent.PolygonCloud] = PdfName.PolygonCloud.StringValue,
                [MarkupIntent.PolygonDimension] = PdfName.PolygonDimension.StringValue,
                [MarkupIntent.PolyLine] = PdfName.PolyLine.StringValue,
                [MarkupIntent.PolyLineDimension] = PdfName.PolyLineDimension.StringValue
            };
        }

        public static MarkupIntent? Get(string name)
        {
            if (name == null)
                return null;

            return codes.GetKey(name);
        }

        public static PdfName GetCode(this MarkupIntent? intent)
        {
            return intent == null ? null : PdfName.Get(codes[intent.Value], true);
        }
    }
}

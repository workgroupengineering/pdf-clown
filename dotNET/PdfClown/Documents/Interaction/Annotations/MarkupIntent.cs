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
}

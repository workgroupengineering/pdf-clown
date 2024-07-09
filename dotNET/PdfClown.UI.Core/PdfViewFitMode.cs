using System.Runtime.Serialization;

namespace PdfClown.UI
{
    public enum PdfViewFitMode
    {
        [EnumMember(Value = "Page Size")]
        PageSize,
        [EnumMember(Value = "Page Width")]
        PageWidth,
        [EnumMember(Value = "Document Width")]
        DocumentWidth,
        [EnumMember(Value = "Zoom")]
        Zoom
    }
}

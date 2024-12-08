using System.IO;
using System.Threading.Tasks;

namespace PdfClown.UI.Sample
{
    public interface IOpenFileService
    {
        Task<(Stream Stream, string FileName)> OpenFileDialog();
    }
}

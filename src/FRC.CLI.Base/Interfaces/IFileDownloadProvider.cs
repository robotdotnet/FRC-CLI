using System.IO;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IFileDownloadProvider
    {
        Task DownloadFileToFileAsync(string url, string outputLocation, string outputFileName);
        Task DownloadFileToStreamAsync(string url, Stream outputStream);
    }
}
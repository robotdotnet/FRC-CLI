using System.IO;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IFileDownloadProvider
    {
        Task DownloadFileToStreamAsync(string url, Stream outputStream);
    }
}
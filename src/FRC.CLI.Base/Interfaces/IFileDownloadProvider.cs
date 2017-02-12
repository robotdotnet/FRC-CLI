using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IFileDownloadProvider
    {
        Task DownloadFileAsync(string url, string outputLocation, string md5Sum);
    }
}
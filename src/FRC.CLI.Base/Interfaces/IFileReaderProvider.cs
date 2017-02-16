using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IFileReaderProvider
    {
        Task<string> ReadFileAsStringAsync(string file);
    }
}
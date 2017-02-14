using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IOutputWriter
    {
        Task WriteLineAsync(string line);
    }
}
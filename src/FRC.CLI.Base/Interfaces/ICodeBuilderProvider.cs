using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface ICodeBuilderProvider
    {
        Task BuildCodeAsync();
    }
}
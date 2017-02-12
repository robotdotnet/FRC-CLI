using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IWPILibUserFolderResolver
    {
         Task<string> GetWPILibUserFolderAsync();
    }
}
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IProjectInformationProvider
    {
         Task<string> GetProjectRootDirectoryAsync();
         Task<string> GetProjectBuildDirectoryAsync();
         Task<string> GetProjectExecutableNameAsync();
    }
}
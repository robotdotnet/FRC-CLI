using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IProjectInformationProvider
    {
         Task<string> GetProjectRootDirectoryAsync();
         Task<string> GetProjectBuildDirectoryAsync();
         Task<string> GetProjectExecutableNameAsync();
         Task<List<string>> GetFrcDependenciesAsync();
         Task SetFrcDependenciesAsync(IList<(string dep, string version)> dependencies);
         Task SetFrcTooling((string tool, string version) tool);
    }
}
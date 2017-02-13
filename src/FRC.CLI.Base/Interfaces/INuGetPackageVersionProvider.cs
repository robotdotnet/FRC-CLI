using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface INuGetPackageVersionProvider
    {
        Task<List<string>> GetPackageVersionsAsync(string packageName);
    }
}
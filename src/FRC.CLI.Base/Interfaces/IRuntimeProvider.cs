using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRuntimeProvider
    {
         Task InstallRuntimeAsync();
         Task InstallRuntimeAsync(string location);
         Task DownladRuntimeAsync();
         Task VerifyRuntimeAsync();
    }
}
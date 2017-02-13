using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IMonoInstallCheckerProvider
    {
         Task<bool> CheckMonoInstallAsync();
    }
}
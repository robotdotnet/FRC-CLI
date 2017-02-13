using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IMonoPackageInstallerProvider
    {
         Task InstallMonoAsync(string localFile);
    }
}
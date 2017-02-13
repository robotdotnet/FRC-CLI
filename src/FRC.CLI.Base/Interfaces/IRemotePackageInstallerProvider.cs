using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRemotePackageInstallerProvider
    {
        Task InstallZippedPackages(string localFile);
    }
}
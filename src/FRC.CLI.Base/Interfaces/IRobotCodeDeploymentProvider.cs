using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRobotCodeDeploymentProvider
    {
         Task<bool> DeployRobotCodeAsync(string localCodeLocation, IFileDeployerProvider fileDeployerProvider);
         Task<bool> StartRobotCodeAsync(string executableName, IFileDeployerProvider fileDeployerProvider);
    }
}
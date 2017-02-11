using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRobotCodeDeploymentProvider
    {
         Task<bool> DeployRobotCodeAsync(IFileDeployerProvider fileDeployerProvider);
         Task<bool> StartRobotCodeAsync(IFileDeployerProvider fileDeployerProvider);
    }
}
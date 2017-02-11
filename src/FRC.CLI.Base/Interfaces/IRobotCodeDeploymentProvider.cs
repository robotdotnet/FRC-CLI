using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRobotCodeDeploymentProvider
    {
         Task<bool> DeployRobotCodeAsync();
         Task<bool> StartRobotCodeAsync();
    }
}
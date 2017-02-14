using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRobotCodeDeploymentProvider
    {
         Task DeployRobotCodeAsync();
         Task StartRobotCodeAsync();
    }
}
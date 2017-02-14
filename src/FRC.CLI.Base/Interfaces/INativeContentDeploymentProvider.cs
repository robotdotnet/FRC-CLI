using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface INativeContentDeploymentProvider
    {
        string NativeDirectory { get; }

        /// <summary>
        /// Deploys native files to the robot
        /// </summary>
        /// <returns>True if the files were deployed successfully, otherwise false</returns>
        Task DeployNativeContentAsync();
    }
}
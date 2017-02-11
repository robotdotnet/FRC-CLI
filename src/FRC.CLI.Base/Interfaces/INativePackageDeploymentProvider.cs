using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface INativePackageDeploymentProvider
    {
        /// <summary>
        /// Deploys native files to the robot
        /// </summary>
        /// <returns>True if the files were deployed successfully, otherwise false</returns>
        Task<bool> DeployNativeFilesAsync(IFileDeployerProvider fileDeployerProvider);
    }
}
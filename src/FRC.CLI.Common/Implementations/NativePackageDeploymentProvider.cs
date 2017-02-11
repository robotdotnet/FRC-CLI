using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class NativePackageDeploymentProvider : INativePackageDeploymentProvider
    {
        public Task<bool> DeployNativeFilesAsync(IFileDeployerProvider fileDeployerProvider)
        {
            
        }
    }
}
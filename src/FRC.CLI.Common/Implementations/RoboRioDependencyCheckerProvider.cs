using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using Renci.SshNet;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioDependencyCheckerProvider : IRoboRioDependencyCheckerProvider
    {
        public async Task<bool> CheckIfDependenciesAreSatisfied(IFileDeployerProvider fileDeployerProvider)
        {
            // Check mono install
            string checkString = $"test -e {DeployProperties.RoboRioMonoBin}";
            var retVal = await fileDeployerProvider.RunCommandsAsync(new string[] {checkString}, ConnectionUser.LvUser).ConfigureAwait(false);
            SshCommand command;
            if (retVal.TryGetValue(checkString, out command))
            {
                return command.ExitStatus == 0;
            }
            return false;
        }
    }
}
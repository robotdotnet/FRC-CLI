using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using Renci.SshNet;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioDependencyCheckerProvider : IRoboRioDependencyCheckerProvider
    {
        private IFileDeployerProvider m_fileDeployerProvider;
        public RoboRioDependencyCheckerProvider(IFileDeployerProvider fileDeployerProvider)
        {
            m_fileDeployerProvider = fileDeployerProvider;
        }

        public async Task<bool> CheckIfDependenciesAreSatisfied()
        {
            // Check mono install
            string checkString = $"test -e {DeployProperties.RoboRioMonoBin}";
            var retVal = await m_fileDeployerProvider.RunCommandsAsync(new string[] {checkString}, ConnectionUser.LvUser).ConfigureAwait(false);
            SshCommand command;
            if (retVal.TryGetValue(checkString, out command))
            {
                return command.ExitStatus == 0;
            }
            return false;
        }
    }
}
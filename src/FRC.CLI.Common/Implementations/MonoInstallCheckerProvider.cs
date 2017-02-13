using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using Renci.SshNet;

namespace FRC.CLI.Common.Implementations
{
    public class MonoInstallCheckerProvider : IMonoInstallCheckerProvider
    {
        IFileDeployerProvider m_fileDeployerProvider;

        public MonoInstallCheckerProvider(IFileDeployerProvider fileDeployerProvider)
        {
            m_fileDeployerProvider = fileDeployerProvider;
        }

        public async Task<bool> CheckMonoInstallAsync()
        {
            string checkString = $"test -e {DeployProperties.RoboRioMonoBin}";
            var retVal = await m_fileDeployerProvider.RunCommandsAsync(new string[] {checkString}, ConnectionUser.LvUser).ConfigureAwait(false);
            SshCommand command;
            if (retVal.TryGetValue(checkString, out command))
            {
                if (command.ExitStatus == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
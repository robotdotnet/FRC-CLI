using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using Renci.SshNet;

namespace FRC.CLI.Common.Implementations
{
    public class MonoPackageInstallerProvider : IMonoPackageInstallerProvider
    {
        IRemotePackageInstallerProvider m_remotePackageInstallerProvider;
        IFileDeployerProvider m_fileDeployerProvider;
        IExceptionThrowerProvider m_exceptionThrowerProvider;
        IMonoInstallCheckerProvider m_monoInstallCheckerProvider;

        public MonoPackageInstallerProvider(IRemotePackageInstallerProvider remotePackageInstallerProvider,
            IFileDeployerProvider fileDeployerProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IMonoInstallCheckerProvider monoInstallCheckerProvider)
        {
            m_remotePackageInstallerProvider = remotePackageInstallerProvider;
            m_fileDeployerProvider = fileDeployerProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_monoInstallCheckerProvider = monoInstallCheckerProvider;
        }

        public async Task InstallMonoAsync(string localFile)
        {
            await m_remotePackageInstallerProvider.InstallZippedPackagesAsync(localFile).ConfigureAwait(false);

            if (!await m_monoInstallCheckerProvider.CheckMonoInstallAsync().ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("Mono did not install successfully. Please try again");
            }
            
            // Set allow realtime on Mono instance
            await m_fileDeployerProvider.RunCommandsAsync(new string[] {"setcap cap_sys_nice=pe /usr/bin/mono-sgen"},
                ConnectionUser.Admin).ConfigureAwait(false);
        }
    }
}
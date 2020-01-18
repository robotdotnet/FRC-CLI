using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class RemotePackageInstallerProvider : IRemotePackageInstallerProvider
    {
        public const string RoboRioOpgkLocation = "/home/admin/opkg";

        public static readonly string OpkgInstallCommand = $"opkg install {RoboRioOpgkLocation}/*.ipk";
        private readonly IOutputWriter m_outputWriter;
        private readonly IFileDeployerProvider m_fileDeployerProvider;
        private readonly ITeamNumberProvider m_teamNumberProvider;
        private readonly IBuildSettingsProvider m_buildSettingsProvider;
        private readonly IExceptionThrowerProvider m_exceptionThrowerProvider;

        public RemotePackageInstallerProvider(IOutputWriter outputWriter,
            IFileDeployerProvider fileDeployerProvider,
            ITeamNumberProvider teamNumberProvider,
            IBuildSettingsProvider buildSettingsProvider,
            IExceptionThrowerProvider exceptionThrowerProvider)
        {
            m_outputWriter = outputWriter;
            m_fileDeployerProvider = fileDeployerProvider;
            m_buildSettingsProvider = buildSettingsProvider;
            m_teamNumberProvider = teamNumberProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
        }

        public async Task InstallZippedPackagesAsync(string localFile)
        {
            if (!File.Exists(localFile))
            {
                throw m_exceptionThrowerProvider.ThrowException($"Could not find file to deploy: {localFile}");
            }

            await m_outputWriter.WriteLineAsync($"Deploying file: {localFile} to {RoboRioOpgkLocation}").ConfigureAwait(false);

            bool verbose = m_buildSettingsProvider.Verbose;

            var ret = await m_fileDeployerProvider.DeployFilesAsync(new (string, string)[] {(localFile, RoboRioOpgkLocation)},
                            ConnectionUser.Admin).ConfigureAwait(false);

            if (!ret)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to deploy file");
            }

            if (verbose)
            {
                await m_outputWriter.WriteLineAsync("Installing Packages").ConfigureAwait(false);
            }

            List<string> installCommands = new List<string>()
            {
                $"unzip {RoboRioOpgkLocation}/{Path.GetFileName(localFile)} -d {RoboRioOpgkLocation}",
                OpkgInstallCommand
            };

            await m_fileDeployerProvider.RunCommandsAsync(installCommands, ConnectionUser.Admin).ConfigureAwait(false);

            //Removing ipk files from the RoboRIO
            await m_fileDeployerProvider.RunCommandAsync($"rm -rf {RoboRioOpgkLocation}",
                ConnectionUser.Admin).ConfigureAwait(false);

            if (verbose)
            {
                await m_outputWriter.WriteLineAsync($"Finished deploying package {localFile}").ConfigureAwait(false);
            }
        }
    }
}
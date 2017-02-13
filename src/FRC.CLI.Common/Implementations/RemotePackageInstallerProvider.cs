using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using Renci.SshNet;

namespace FRC.CLI.Common.Implementations
{
    public class RemotePackageInstallerProvider : IRemotePackageInstallerProvider
    {
        IOutputWriter m_outputWriter;
        IFileDeployerProvider m_fileDeployerProvider;
        ITeamNumberProvider m_teamNumberProvider;
        IBuildSettingsProvider m_buildSettingsProvider;
        IExceptionThrowerProvider m_exceptionThrowerProvider;

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

        public async Task InstallZippedPackages(string localFile)
        {
            if (!File.Exists(localFile))
            {
                throw m_exceptionThrowerProvider.ThrowException($"Could not find file to deploy: {localFile}");
            }


            await m_outputWriter.WriteLineAsync($"Deploying file: {localFile} to {DeployProperties.RoboRioOpgkLocation}").ConfigureAwait(false);
            
            bool verbose = m_buildSettingsProvider.Verbose;

            if (verbose)
            {
                await m_outputWriter.WriteLineAsync("Creating Opkg Directory").ConfigureAwait(false);
            }

            await m_fileDeployerProvider.RunCommandsAsync(new string[] {$"mkdir -p {DeployProperties.RoboRioOpgkLocation}"},
                            ConnectionUser.Admin).ConfigureAwait(false);
            
            var ret = await m_fileDeployerProvider.DeployFilesAsync(new string[] {localFile}, DeployProperties.RoboRioOpgkLocation,
                            ConnectionUser.Admin).ConfigureAwait(false);

            if (!ret)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to deploy file");
            }

            if (verbose)
            {
                await m_outputWriter.WriteLineAsync("Installing mono").ConfigureAwait(false);
            }

            List<string> installCommands = new List<string>()
            {
                $"unzip {DeployProperties.RoboRioOpgkLocation}/{Path.GetFileName(localFile)} -d {DeployProperties.RoboRioOpgkLocation}",
                DeployProperties.OpkgInstallCommand
            };

            await m_fileDeployerProvider.RunCommandsAsync(installCommands, ConnectionUser.Admin);

            // Check mono install
            string checkString = $"test -e {DeployProperties.RoboRioMonoBin}";
            var retVal = await m_fileDeployerProvider.RunCommandsAsync(new string[] {checkString}, ConnectionUser.LvUser).ConfigureAwait(false);
            SshCommand command;
            if (retVal.TryGetValue(checkString, out command))
            {
                if (command.ExitStatus != 0)
                {
                    throw m_exceptionThrowerProvider.ThrowException("Mono did not install successfully. Please try again");
                }
            }
            else
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to deploy");
            }

            // Set allow realtime on Mono instance
            await m_fileDeployerProvider.RunCommandsAsync(new string[] {"setcap cap_sys_nice=pe /usr/bin/mono-sgen"},
                ConnectionUser.Admin).ConfigureAwait(false);

            //Removing ipk files from the RoboRIO
            await m_fileDeployerProvider.RunCommandsAsync(new string[] {$"rm -rf {DeployProperties.RoboRioOpgkLocation}"},
                ConnectionUser.Admin).ConfigureAwait(false);

            await m_outputWriter.WriteLineAsync("Done. you may now deploy code to your robot.");
        }
    }
}
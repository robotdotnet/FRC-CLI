using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class MonoRuntimeProvider : IRuntimeProvider
    {
        private IRemotePackageInstallerProvider m_remotePackageInstallerProvider;
        private IFileDeployerProvider m_fileDeployerProvider;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;
        private IWPILibUserFolderResolver m_wpilibUserFolderResolver;
        private IFileDownloadProvider m_fileDownloadProvider;
        private IMd5HashCheckerProvider m_md5HashCheckerProvider;
        private IOutputWriter m_outputWriter;

        public string MonoVersion = DeployProperties.MonoVersion;
        public string MonoUrl = DeployProperties.MonoUrl;

        public string MonoMd5 = DeployProperties.MonoMd5;

        public MonoRuntimeProvider(IRemotePackageInstallerProvider remotePackageInstallerProvider,
            IFileDeployerProvider fileDeployerProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IWPILibUserFolderResolver wpilibUserFolderResolver,
            IFileDownloadProvider fileDownloadProvider,
            IMd5HashCheckerProvider md5HashCheckerProvider,
            IOutputWriter outputWriter)
        {
            m_remotePackageInstallerProvider = remotePackageInstallerProvider;
            m_fileDeployerProvider = fileDeployerProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_wpilibUserFolderResolver = wpilibUserFolderResolver;
            m_fileDownloadProvider = fileDownloadProvider;
            m_md5HashCheckerProvider = md5HashCheckerProvider;
            m_outputWriter = outputWriter;
        }

        public async System.Threading.Tasks.Task DownladRuntimeAsync()
        {
            await m_outputWriter.WriteLineAsync("Downloading Mono Runtime");
            string monoFolder = await GetMonoFolder();
            string monoFilePath = Path.Combine(monoFolder, MonoVersion);
            if (await m_md5HashCheckerProvider.VerifyMd5Hash(monoFilePath, MonoMd5))
            {
                // File already exists and is correct. No need to redownload
                await m_outputWriter.WriteLineAsync("Runtime already downloaded. Skipping...");
                return;
            }
            await m_fileDownloadProvider.DownloadFileToFileAsync(MonoUrl + MonoVersion,
                monoFolder, MonoVersion);
            if (!await m_md5HashCheckerProvider.VerifyMd5Hash(monoFilePath, MonoMd5))
            {
                throw m_exceptionThrowerProvider.ThrowException("Mono file not downloaded successfully");
            }
        }

        private async Task<string> GetMonoFolder()
        {
            var wpilibFolder = await m_wpilibUserFolderResolver.GetWPILibUserFolderAsync();
            var monoFolder = Path.Combine(wpilibFolder, "mono");
            return monoFolder;
        }

        public async System.Threading.Tasks.Task InstallRuntimeAsync()
        {
            string monoFolder = await GetMonoFolder();
            string monoFilePath = Path.Combine(monoFolder, MonoVersion);
            await InstallRuntimeAsync(monoFilePath);
        }

        public async Task InstallRuntimeAsync(string location)
        {
            await m_outputWriter.WriteLineAsync("Installing Mono Runtime");
            if (!await m_md5HashCheckerProvider.VerifyMd5Hash(location, MonoMd5))
            {
                throw m_exceptionThrowerProvider.ThrowException("Mono file could not be verified. Please redownload and try again");
            }

            await m_remotePackageInstallerProvider.InstallZippedPackagesAsync(location);
            
            await VerifyRuntimeAsync();

            var command = await m_fileDeployerProvider.RunCommandAsync("setcap cap_sys_nice=pe /usr/bin/mono-sgen",
                ConnectionUser.Admin).ConfigureAwait(false);

            if (command == null || command.ExitStatus != 0)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to set RealTime capabilities on Mono");
            }
            await m_outputWriter.WriteLineAsync("Successfully installed Mono Runtime");
        }

        public async System.Threading.Tasks.Task VerifyRuntimeAsync()
        {
            await m_outputWriter.WriteLineAsync("Verifying Mono Runtime").ConfigureAwait(false);
            string checkString = $"test -e {DeployProperties.RoboRioMonoBin}";
            var command = await m_fileDeployerProvider.RunCommandAsync(checkString, ConnectionUser.LvUser).ConfigureAwait(false);
            if (command == null || command.ExitStatus != 0)
            {
                throw m_exceptionThrowerProvider.ThrowException("Mono runtime not installed. Please try reinstalling");
            }
            await m_outputWriter.WriteLineAsync("Mono runtime correctly installed");
        }
    }
}
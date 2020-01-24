using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class MonoRuntimeProvider : IRuntimeProvider
    {
        private readonly IRemotePackageInstallerProvider m_remotePackageInstallerProvider;
        private readonly IFileDeployerProvider m_fileDeployerProvider;
        private readonly IExceptionThrowerProvider m_exceptionThrowerProvider;
        private readonly IWPILibUserFolderResolver m_wpilibUserFolderResolver;
        private readonly IFileDownloadProvider m_fileDownloadProvider;
        private readonly IMd5HashCheckerProvider m_md5HashCheckerProvider;
        private readonly IOutputWriter m_outputWriter;

        public string MonoZipName = DeployProperties.MonoZipName;
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

        public virtual async Task DownloadToFileAsync(string url, string file)
        {
            await m_outputWriter.WriteLineAsync($"Writing file to: {file}").ConfigureAwait(false);
            using var writeStream = File.Open(file, FileMode.Create);
            await m_fileDownloadProvider.DownloadFileToStreamAsync(
url, writeStream).ConfigureAwait(false);
        }

        public async System.Threading.Tasks.Task DownladRuntimeAsync()
        {
            await m_outputWriter.WriteLineAsync("Downloading Mono Runtime").ConfigureAwait(false);
            string monoFolder = await GetMonoFolderAsync().ConfigureAwait(false);
            string monoFilePath = Path.Combine(monoFolder, MonoZipName);
            if (await m_md5HashCheckerProvider.VerifyMd5Hash(monoFilePath, MonoMd5).ConfigureAwait(false))
            {
                // File already exists and is correct. No need to redownload
                await m_outputWriter.WriteLineAsync("Runtime already downloaded. Skipping...").ConfigureAwait(false);
                return;
            }
            Directory.CreateDirectory(monoFolder);
            string file = Path.Combine(monoFolder, MonoZipName);
            await DownloadToFileAsync(MonoUrl, file).ConfigureAwait(false);
            if (!await m_md5HashCheckerProvider.VerifyMd5Hash(monoFilePath, MonoMd5).ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("Mono file not downloaded successfully");
            }
        }

        public virtual async Task<string> GetMonoFolderAsync()
        {
            var wpilibFolder = await m_wpilibUserFolderResolver.GetWPILibUserFolderAsync().ConfigureAwait(false);
            var monoFolder = Path.Combine(wpilibFolder, "mono");
            return monoFolder;
        }

        public async System.Threading.Tasks.Task InstallRuntimeAsync()
        {
            string monoFolder = await GetMonoFolderAsync().ConfigureAwait(false);
            string monoFilePath = Path.Combine(monoFolder, MonoZipName);
            await InstallRuntimeAsync(monoFilePath).ConfigureAwait(false);
        }

        public virtual async Task InstallRuntimeAsync(string location)
        {
            await m_outputWriter.WriteLineAsync("Installing Mono Runtime").ConfigureAwait(false);
            if (!await m_md5HashCheckerProvider.VerifyMd5Hash(location, MonoMd5).ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("Mono file could not be verified. Please redownload and try again");
            }

            await m_remotePackageInstallerProvider.InstallZippedPackagesAsync(location).ConfigureAwait(false);

            await VerifyRuntimeAsync().ConfigureAwait(false);

            var command = await m_fileDeployerProvider.RunCommandAsync("setcap cap_sys_nice=pe /usr/bin/mono-sgen",
                ConnectionUser.Admin).ConfigureAwait(false);

            if (command == null || command.ExitStatus != 0)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to set RealTime capabilities on Mono");
            }
            await m_outputWriter.WriteLineAsync("Successfully installed Mono Runtime").ConfigureAwait(false);
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
            await m_outputWriter.WriteLineAsync("Mono runtime correctly installed").ConfigureAwait(false);
        }
    }
}
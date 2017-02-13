using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;

namespace dotnet_frc
{
    class MonoCommand : FrcSubCommandBase
    {

        private CommandOption _downloadOption;
        private CommandOption _installOption;
        private CommandOption _locationOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new MonoCommand
            {
                Name = "mono",
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command);

            command._downloadOption = command.Option(
                "-d|--download <DOWNLOAD>",
                "Download mono",
                CommandOptionType.NoValue
            );

            command._installOption = command.Option(
                "-i|--install <Install>",
                "Install mono",
                CommandOptionType.NoValue
            );

            command._locationOption = command.Option(
                "-l|--location",
                "Local mono location",
                CommandOptionType.SingleValue
            );

            return command;
        }

        public override async Task<int> RunAsync(string fileOrDirectory)
        {
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
                false, _verboseOption.HasValue());            
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                bool downloaded = false;
                var wpiLibFolder = await scope.Resolve<IWPILibUserFolderResolver>().GetWPILibUserFolderAsync().ConfigureAwait(false);
                    wpiLibFolder = Path.Combine(wpiLibFolder, "mono");
                var wpilibDownloadConstants = scope.Resolve<IMonoFileConstantsProvider>();
                string downloadLocation = Path.Combine(wpiLibFolder, wpilibDownloadConstants.OutputFileName);
                if (_downloadOption.HasValue())
                {
                    await scope.Resolve<IFileDownloadProvider>().DownloadFileAsync(wpilibDownloadConstants.Url, 
                        wpiLibFolder, wpilibDownloadConstants.OutputFileName).ConfigureAwait(false);
                    string sum = await MD5Helper.Md5SumAsync(downloadLocation).ConfigureAwait(false);
                    if (sum == null || sum != wpilibDownloadConstants.Md5Sum)
                    {
                        throw scope.Resolve<IExceptionThrowerProvider>().ThrowException("File not downloaded properly");
                    }
                    await scope.Resolve<IOutputWriter>().WriteLineAsync("Successfully downloaded mono. Run with -i argument to deploy to robot");
                    downloaded = true;
                }
                if (_installOption.HasValue())
                {
                    Console.WriteLine("2");
                    if (downloaded)
                    {
                        Console.WriteLine("3");
                        // Downloaded previously. No need to check file. Just deploy it.
                        await scope.Resolve<IRemotePackageInstallerProvider>().InstallZippedPackagesAsync(downloadLocation).ConfigureAwait(false);
                    }
                }

            }

            return 0;
        }

        
    }
}
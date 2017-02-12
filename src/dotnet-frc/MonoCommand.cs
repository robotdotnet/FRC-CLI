using System.IO;
using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
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
                "-l|--location <>",
                "Local mono location",
                CommandOptionType.SingleValue
            );

            return command;
        }

        public override async Task<int> RunAsync(string fileOrDirectory)
        {
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this);
            
            builder.Register(c => new DotNetBuildSettingsProvider(false, _verboseOption.HasValue())).As<IBuildSettingsProvider>();
            
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
            if (_downloadOption.HasValue())
            {
                var wpiLibFolder = await scope.Resolve<IWPILibUserFolderResolver>().GetWPILibUserFolderAsync().ConfigureAwait(false);
                wpiLibFolder = Path.Combine(wpiLibFolder, "mono");
                var wpilibDownloadConstants = scope.Resolve<IMonoFileConstantsProvider>();
                await scope.Resolve<IFileDownloadProvider>().DownloadFileAsync(wpilibDownloadConstants.Url, wpiLibFolder, wpilibDownloadConstants.Md5Sum).ConfigureAwait(false);
            }

            }

            return 0;
        }

        
    }
}
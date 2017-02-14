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
                Description = "Installs mono on the robot",
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command);

            command._downloadOption = command.Option(
                "-d|--download",
                "Download mono",
                CommandOptionType.NoValue
            );

            command._installOption = command.Option(
                "-i|--install",
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
                false);            
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                if (!_downloadOption.HasValue() && !_installOption.HasValue())
                {
                    throw scope.Resolve<IExceptionThrowerProvider>().ThrowException(
                        "No argument specified. Must provide an argument to use"
                    );
                }

                var runtimeProvider = scope.Resolve<IRuntimeProvider>();
                if (_downloadOption.HasValue())
                {
                    await runtimeProvider.DownladRuntimeAsync();
                }

                if (_installOption.HasValue())
                {
                    if (_locationOption.HasValue())
                    {
                        await runtimeProvider.InstallRuntimeAsync(_locationOption.Value());
                    }
                    else
                    {
                        await runtimeProvider.InstallRuntimeAsync();
                    }
                }
            }

            return 0;
        }

        
    }
}
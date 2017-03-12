using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common.Implementations;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;

namespace dotnet_frc
{
    internal class RuntimeCommand : FrcSubCommandBase
    {

        private CommandOption _downloadOption;
        private CommandOption _installOption;
        private CommandOption _locationOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new RuntimeCommand
            {
                Name = "runtime",
                Description = "Installs the runtime on the robot",
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command);

            command._downloadOption = command.Option(
                "-d|--download",
                "Download the runtime",
                CommandOptionType.NoValue
            );

            command._installOption = command.Option(
                "-i|--install",
                "Install the runtime",
                CommandOptionType.NoValue
            );

            command._locationOption = command.Option(
                "-l|--location",
                "Local runtime location",
                CommandOptionType.SingleValue
            );

            return command;
        }

        public override async Task<int> RunAsync(string fileOrDirectory)
        {
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
                false);
            builder.RegisterType<MonoRuntimeProvider>().As<IRuntimeProvider>();
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
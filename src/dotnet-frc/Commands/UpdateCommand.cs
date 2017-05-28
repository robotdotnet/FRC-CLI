using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common.Implementations;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;

namespace dotnet_frc
{
    internal class UpdateCommand : FrcSubCommandBase
    {
        private CommandOption _toolsOption;
        private CommandOption _dependenciesOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new UpdateCommand
            {
                Name = "update",
                Description = "Updates tools and WPILib dependencies in the project",
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command);

            command._toolsOption = command.Option(
                "-t|--tools",
                "Update the tooling",
                CommandOptionType.NoValue
            );

            command._dependenciesOption = command.Option(
                "-d|--dependencies",
                "Update the dependencies",
                CommandOptionType.NoValue
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
                if (!_toolsOption.HasValue() && !_dependenciesOption.HasValue())
                {
                    throw scope.Resolve<IExceptionThrowerProvider>().ThrowException(
                        "No argument specified. Must provide an argument to use"
                    );
                }
            }
            return 0;
        }
    }
}
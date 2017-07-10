using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;
using System.Threading.Tasks;
using FRC.CLI.Common;
using Autofac;

namespace dotnet_frc
{
    internal class DeployCommand : FrcSubCommandBase
    {
        private CommandOption _debugOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new DeployCommand
            {
                Name = "deploy",
                Description = "Deploys code to the robot",
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command);

            command._debugOption = command.Option(
                "-d|--debug <DEBUG>",
                "Debugger mode",
                CommandOptionType.NoValue
            );

            return command;
        }

        public override async Task<int> RunAsync(string fileOrDirectory)
        {
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
                _debugOption.HasValue());
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var deployer = scope.Resolve<CodeDeployer>();
                await deployer.DeployCode();
            }
            return 0;
        }
    }
}
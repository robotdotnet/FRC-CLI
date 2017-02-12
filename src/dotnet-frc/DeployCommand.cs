using Microsoft.Build.Evaluation;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;
using System.Threading.Tasks;
using Nito.AsyncEx;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common;
using Autofac;

namespace dotnet_frc
{
    internal class DeployCommand : DotNetSubCommandBase
    {
        private CommandOption _debugOption;
        private CommandOption _teamOption;
        private CommandOption _verboseOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new DeployCommand
            {
                Name = "deploy",
                HandleRemainingArguments = false
            };

            command.HelpOption("-h|--help");

            command._debugOption = command.Option(
                "-d|--debug <DEBUG>",
                "Debugger mode",
                CommandOptionType.NoValue
            );

            command._teamOption = command.Option(
                "-t|--team",
                "Force a team",
                CommandOptionType.SingleValue
            );

            command._verboseOption = command.Option(
                "-v|--verbose <VERBOSE>",
                "Verbose output",
                CommandOptionType.NoValue
            );

            return command;
        }

        public async Task<int> RunAsync(string fileOrDirectory)
        {
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder);
            builder.Register(c =>
            {
                MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, fileOrDirectory);
                return new DotNetProjectInformationProvider(msBuild);
            }).As<IProjectInformationProvider>().As<DotNetProjectInformationProvider>();
            builder.Register(c => new DotNetBuildSettingsProvider(_debugOption.HasValue(), _verboseOption.HasValue())).As<IBuildSettingsProvider>();
            builder.RegisterType<DotNetTeamNumberProvider>().As<ITeamNumberProvider>().WithParameter(new TypedParameter(typeof(int?), 
                DotNetTeamNumberProvider.GetTeamNumberFromCommandOption(_teamOption)));
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var deployer = scope.Resolve<CodeDeployer>();
                await deployer.DeployCode();
            }
            return 0;
        }

        public override int Run(string fileOrDirectory)
        {
            return AsyncContext.Run(async () => await RunAsync(fileOrDirectory).ConfigureAwait(false));
        }
    }
}
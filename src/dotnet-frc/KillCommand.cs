using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Common;
using Microsoft.Build.Evaluation;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;
using Nito.AsyncEx;
using Autofac;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    class KillCommand : DotNetSubCommandBase
    {
        private CommandOption _teamOption;
        private CommandOption _verboseOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new KillCommand
            {
                Name = "kill",
                HandleRemainingArguments = false
            };

            command.HelpOption("-h|--help");
            
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
            builder.RegisterType<DotNetExceptionThrowerProvider>().As<IExceptionThrowerProvider>();
            builder.Register(c =>
            {
                MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, fileOrDirectory);
                return new DotNetProjectInformationProvider(msBuild);
            }).As<IProjectInformationProvider>();
            builder.Register(c => new DotNetBuildSettingsProvider(false, _verboseOption.HasValue())).As<IBuildSettingsProvider>();
            builder.RegisterType<DotNetTeamNumberProvider>().As<ITeamNumberProvider>().WithParameter(new TypedParameter(typeof(int?), 
                DotNetTeamNumberProvider.GetTeamNumberFromCommandOption(_teamOption)));
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var rioConn = scope.Resolve<IFileDeployerProvider>();
                await rioConn.RunCommandsAsync(new string[] { DeployProperties.KillOnlyCommand}, ConnectionUser.LvUser);
            }
            return 0;
        }

        public override int Run(string fileOrDirectory)
        {
            return AsyncContext.Run(async () => await RunAsync(fileOrDirectory).ConfigureAwait(false));
        }
    }
}
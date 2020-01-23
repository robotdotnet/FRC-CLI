using System.Threading.Tasks;
using FRC.CLI.Common;
using Autofac;
using System.CommandLine.Invocation;

namespace dotnet_frc.Commands
{
    internal class DeployCommand : SubCommandBase
    {
//#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
//        private CommandOption _debugOption;
//#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

//        public static DotNetSubCommandBase Create()
//        {
//            var command = new DeployCommand
//            {
//                Name = "deploy",
//                Description = "Deploys code to the robot",
//                HandleRemainingArguments = false
//            };

//            SetupBaseOptions(command);

//            command._debugOption = command.Option(
//                "-d|--debug <DEBUG>",
//                "Debugger mode",
//                CommandOptionType.NoValue
//            );

//            return command;
//        }

        public DeployCommand() : base("deploy", "Deploy robot code")
        {
            Handler = CommandHandler.Create<int, string>(HandleCommand);
        }

        public async Task<int> HandleCommand(int team, string? project)
        {
            var msBuild = ResolveProject(project);
            if (msBuild == null)
            {

                return -1;
            }
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, msBuild, team, false,
                false);
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var deployer = scope.Resolve<CodeDeployer>();
                await deployer.DeployCode();
            }
            return 0;
        }

        //public override async Task<int> RunAsync(string fileOrDirectory)
        //{
        //    var builder = new ContainerBuilder();
        //    AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
        //        _debugOption.HasValue());
        //    var container = builder.Build();

        //    using (var scope = container.BeginLifetimeScope())
        //    {
        //        var deployer = scope.Resolve<CodeDeployer>();
        //        await deployer.DeployCode();
        //    }
        //    return 0;
        //}
    }
}
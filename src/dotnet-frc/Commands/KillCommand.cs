using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Common;
using Microsoft.DotNet.Cli;
using Autofac;
using FRC.CLI.Base.Interfaces;
using System.CommandLine.Invocation;

namespace dotnet_frc.Commands
{
    internal class KillCommand : SubCommandBase
    {

        public KillCommand() : base("kill", "Kills the currently running code on the project")
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
                // Manually resolve project
                await scope.Resolve<IOutputWriter>().WriteLineAsync("Killing robot code");
                var rioConn = scope.Resolve<IFileDeployerProvider>();
                await rioConn.RunCommandAsync(DeployProperties.KillOnlyCommand, ConnectionUser.LvUser);
                await scope.Resolve<IOutputWriter>().WriteLineAsync("Robot code is kill");
            }
            return 0;
        }

        //public override async Task<int> RunAsync(string fileOrDirectory)
        //{
        //    var builder = new ContainerBuilder();
        //    AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
        //        false);
        //    var container = builder.Build();

        //    using (var scope = container.BeginLifetimeScope())
        //    {
        //        await scope.Resolve<IOutputWriter>().WriteLineAsync("Killing robot code");
        //        var rioConn = scope.Resolve<IFileDeployerProvider>();
        //        await rioConn.RunCommandAsync(DeployProperties.KillOnlyCommand, ConnectionUser.LvUser);
        //    }
        //    return 0;
        //}
    }
}
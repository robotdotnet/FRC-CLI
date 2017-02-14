using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Common;
using Microsoft.DotNet.Cli;
using Autofac;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    class KillCommand : FrcSubCommandBase
    {
        public static DotNetSubCommandBase Create()
        {
            var command = new KillCommand
            {
                Name = "kill",
                Description = "Kills the currently running code on the robot",
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command);

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
                await scope.Resolve<IOutputWriter>().WriteLineAsync("Killing robot code");
                var rioConn = scope.Resolve<IFileDeployerProvider>();
                await rioConn.RunCommandAsync(DeployProperties.KillOnlyCommand, ConnectionUser.LvUser);
            }
            return 0;
        }
    }
}
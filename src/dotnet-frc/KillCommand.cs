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
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command);

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
                var rioConn = scope.Resolve<IFileDeployerProvider>();
                await rioConn.RunCommandsAsync(new string[] { DeployProperties.KillOnlyCommand}, ConnectionUser.LvUser);
            }
            return 0;
        }
    }
}
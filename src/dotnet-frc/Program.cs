using System;
using System.Collections.Generic;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.Utils;


namespace dotnet_frc
{
    public class FrcCommand : DotNetTopLevelCommandBase
    {
        protected override string CommandName => "frc";
        protected override string FullCommandNameLocalized => ".NET FRC Utility";
        protected override string ArgumentName => Constants.ProjectArgumentName;
        protected override string ArgumentDescriptionLocalized => "FRC deploy and setup utility";
        internal override List<Func<DotNetSubCommandBase>> SubCommands =>
            new List<Func<DotNetSubCommandBase>>
            {
                DeployCommand.Create,
                KillCommand.Create
            };

        public static int Run(string[] args)
        {
            var command = new FrcCommand();
            return command.RunCommand(args);
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            /*
            AsyncContext.Run(async () =>
            {
                using (RoboRioConnection rioConn = await RoboRioConnection.StartConnectionTaskAsync(9999, new ConsoleWriter(), new SettingsProvider()))
                {
                    RoboRioImageProvider imageProvider = new RoboRioImageProvider(null);
                    Console.WriteLine(await imageProvider.GetCurrentRoboRioImageAsync(rioConn));
                    ;
                }
            });
            */

            DebugHelper.HandleDebugSwitch(ref args);
            return FrcCommand.Run(args);
        }
    }
}

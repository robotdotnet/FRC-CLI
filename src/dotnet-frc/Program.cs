using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FRC.CLI.Common.Connections;
using FRC.CLI.Common.Implementations;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.Common;
using Microsoft.DotNet.Tools.Restore;
using Nito.AsyncEx;
using NuGet.Frameworks;


namespace dotnet_frc
{
    public class AddCommand : DotNetTopLevelCommandBase
    {
        protected override string CommandName => "frc";
        protected override string FullCommandNameLocalized => ".NET FRC Utility";
        protected override string ArgumentName => Constants.ProjectArgumentName;
        protected override string ArgumentDescriptionLocalized => "FRC deploy and setup utility";
        internal override List<Func<DotNetSubCommandBase>> SubCommands =>
            new List<Func<DotNetSubCommandBase>>
            {
                DeployCommand.Create,
            };

        public static int Run(string[] args)
        {
            var command = new AddCommand();
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
            return AddCommand.Run(args);
        }
    }
}

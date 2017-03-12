using System;
using System.Collections.Generic;
using dotnet_frc.Commands;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.Utils;


namespace dotnet_frc
{
    public class FrcCommand : DotNetTopLevelCommandBase
    {
        protected override string CommandName => "frc";
        protected override string FullCommandNameLocalized => ".NET FRC Utility";
        protected override string ArgumentName => Constants.ProjectArgumentName;
        protected override string ArgumentDescriptionLocalized =>
            "The project file to operate on. If a file is not specified," +
            " the command will search the current directory for one.";
        internal override List<Func<DotNetSubCommandBase>> SubCommands =>
            new List<Func<DotNetSubCommandBase>>
            {
                DeployCommand.Create,
                KillCommand.Create,
                SettingsCommand.Create,
                RuntimeCommand.Create
            };

        public static int Run(string[] args)
        {
            var command = new FrcCommand();
            return command.RunCommand(args);
        }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);
            return FrcCommand.Run(args);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.Common;
using Microsoft.DotNet.Tools.Restore;
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
            DebugHelper.HandleDebugSwitch(ref args);
            return AddCommand.Run(args);
            
            
/*
            

            Microsoft.DotNet.Cli.CommandLine.CommandLineApplication cmd = new Microsoft.DotNet.Cli.CommandLine.CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "frc",
                FullName = ".NET FRC Utility",
                Description = "FRC deploy and setup utility",
                HandleRemainingArguments = true,
                ArgumentSeparatorHelpText = Microsoft.DotNet.Cli.CommandLine.HelpMessageStrings.MSBuildAdditionalArgsHelpText,
            };

            cmd.HelpOption("-h|--help");

            var argRoot = cmd.Argument(
                    $"[{LocalizableStrings.CmdArgument}]",
                    LocalizableStrings.CmdArgumentDescription,
                    multipleValues: true); 

            cmd.OnExecute(() =>
            {
                var msbuildArgs = new List<string>()
                {
                     "/NoLogo", 
                     "/t:Restore", 
                     "/ConsoleLoggerParameters:Verbosity=Minimal" 
                };

                foreach (var i in argRoot.Values)
                {
                    Console.WriteLine(i);
                }

                return 0;
            });

            cmd.Execute(args);

            ProjectCollection collection = new ProjectCollection();
            string loc = @"C:\Users\thadh\Documents\VSTests\src\Robot451";
            MsBuildProject msProject = MsBuildProject.FromFileOrDirectory(collection, loc);

            Console.WriteLine(msProject.ProjectDirectory);
            Console.WriteLine(msProject.ProjectFile);


            Console.WriteLine("Hello World!");
            */
        }
    }
}

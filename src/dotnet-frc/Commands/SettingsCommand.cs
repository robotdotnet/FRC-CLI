using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Base.Models;

namespace dotnet_frc.Commands
{
    public class SettingsCommand : SubCommandBase
    {
        private Option<string[]> ignoreCommand;
        private Option<string[]> argumentCommand;

        public SettingsCommand() : base("settings", "Set FRC Specific Settings")
        {
            ignoreCommand = new Option<string[]>("--ignore", "Add files to ignore on deploy");
            ignoreCommand.AddAlias("-i");
            argumentCommand = new Option<string[]>("--arguments", "Add arguments to be deployed");
            argumentCommand.AddAlias("-a");

            Add(ignoreCommand);
            Add(argumentCommand);


            Handler = CommandHandler.Create<int, string, string[], string[]>(HandleCommand);
        }


        public async Task<int> HandleCommand(int team, string? project, string[]? ignore, string[]? arguments)
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
                var settingsProvider = scope.Resolve<IFrcSettingsProvider>();
                FrcSettings? currentSettings = await settingsProvider.GetFrcSettingsAsync().ConfigureAwait(false);
                if (currentSettings == null)
                {
                    currentSettings = new FrcSettings(team, new List<string>(), new List<string>());
                }

                if (ignore != null)
                {
                    var setVals = ignore.Where(x => !currentSettings.DeployIgnoreFiles.Contains(x));
                    currentSettings.DeployIgnoreFiles.AddRange(setVals);
                }

                if (arguments != null)
                {
                    var setVals = arguments.Where(x => !currentSettings.CommandLineArguments.Contains(x));
                    currentSettings.CommandLineArguments.AddRange(setVals);
                }

                await settingsProvider.WriteFrcSettingsAsync(currentSettings).ConfigureAwait(false);
            }

            return 0;
            /* do something */
        }
    }
    //    internal class SettingsCommand : FrcSubCommandBase
    //    {
    //        private CommandOption _ignoreCommand;
    //        private CommandOption _argumentCommand;

    //#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    //        private SettingsCommand()
    //#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    //        {

    //        }

    //        public static DotNetSubCommandBase Create()
    //        {
    //            var command = new SettingsCommand
    //            {
    //                Name = "settings",
    //                Description = "Set FRC specific settings",
    //                HandleRemainingArguments = false
    //            };

    //            command._ignoreCommand = command.Option(
    //                "-i|--ignore",
    //                "Add files to ignore on deploy",
    //                CommandOptionType.MultipleValue
    //            );

    //            command._argumentCommand = command.Option(
    //                "-a|--arguments",
    //                "Add arguments to be deployed",
    //                CommandOptionType.MultipleValue
    //            );

    //            SetupBaseOptions(command);

    //            command._teamOption.Description =
    //                "Select team number to be set";

    //            return command;
    //        }

    //        public override async Task<int> RunAsync(string fileOrDirectory)
    //        {
    //            var builder = new ContainerBuilder();
    //            AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
    //                false);
    //            var container = builder.Build();

    //            using (var scope = container.BeginLifetimeScope())
    //            {
    //                if (!_teamOption.HasValue() && !_ignoreCommand.HasValue()
    //                                           && !_argumentCommand.HasValue())
    //                {
    //                    throw scope.Resolve<IExceptionThrowerProvider>().ThrowException(
    //                        "No argument specified. Must provide an argument to use"
    //                    );
    //                }

    //                var settingsProvider = scope.Resolve<IFrcSettingsProvider>();
    //                FrcSettings? currentSettings = await settingsProvider.GetFrcSettingsAsync().ConfigureAwait(false);
    //                if (currentSettings == null)
    //                {
    //                    currentSettings = new FrcSettings("-1", new List<string>(), new List<string>());
    //                }

    //                if (_teamOption.HasValue())
    //                {
    //                    if (int.TryParse(_teamOption.Value(), out var teamNumber))
    //                    {
    //                        if (teamNumber > 0)
    //                        {
    //                            currentSettings.TeamNumber = teamNumber.ToString();
    //                        }
    //                    }
    //                }

    //                if (_ignoreCommand.HasValue())
    //                {
    //                    var setVals = _ignoreCommand.Values.Where(x => !currentSettings.DeployIgnoreFiles.Contains(x));
    //                    currentSettings.DeployIgnoreFiles.AddRange(setVals);
    //                }

    //                if (_argumentCommand.HasValue())
    //                {
    //                    var setVals = _argumentCommand.Values.Where(x => !currentSettings.CommandLineArguments.Contains(x));
    //                    currentSettings.CommandLineArguments.AddRange(setVals);
    //                }

    //                await settingsProvider.WriteFrcSettingsAsync(currentSettings).ConfigureAwait(false);
    //            }

    //            return 0;
    //        }
    //    }
}
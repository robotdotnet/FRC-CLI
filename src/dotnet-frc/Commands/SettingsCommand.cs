using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Base.Models;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;

namespace dotnet_frc.Commands
{
    class SettingsCommand : FrcSubCommandBase
    {
        CommandOption _ignoreCommand;
        CommandOption _argumentCommand;
        CommandOption _updateArgument;

        public static DotNetSubCommandBase Create()
        {
            var command = new SettingsCommand
            {
                Name = "settings",
                HandleRemainingArguments = false
            };

            command._ignoreCommand = command.Option(
                "-i|--ignore",
                "Add files to ignore on deploy",
                CommandOptionType.MultipleValue
            );

            command._argumentCommand = command.Option(
                "-a|--arguments",
                "Add arguments to be deployed",
                CommandOptionType.MultipleValue
            );

            command._updateArgument = command.Option(
                "-u|--update",
                "Updates tool to latest version",
                CommandOptionType.NoValue
            );

            SetupBaseOptions(command);
            
            command._teamOption.Description = 
                "Select team number to be set";

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
                if (_updateArgument.HasValue())
                {
                    
                }

                var settingsProvider = scope.Resolve<IFrcSettingsProvider>();
                FrcSettings currentSettings = await settingsProvider.GetFrcSettingsAsync().ConfigureAwait(false);
                if (currentSettings == null)
                {
                    currentSettings = new FrcSettings();
                    currentSettings.TeamNumber = "0";
                    currentSettings.CommandLineArguments = new List<string>();
                    currentSettings.DeployIgnoreFiles = new List<string>();
                }

                if (_teamOption.HasValue())
                {
                    int teamNumber = 0;
                    if (int.TryParse(_teamOption.Value(), out teamNumber))
                    {
                        if (teamNumber > 0)
                        {
                            currentSettings.TeamNumber = teamNumber.ToString();
                        }
                    }
                }

                if (_ignoreCommand.HasValue())
                {
                    var setVals = _ignoreCommand.Values.Where(x => !currentSettings.DeployIgnoreFiles.Contains(x));
                    currentSettings.DeployIgnoreFiles.AddRange(setVals);
                }

                if (_argumentCommand.HasValue())
                {
                    var setVals = _argumentCommand.Values.Where(x => !currentSettings.CommandLineArguments.Contains(x));
                    currentSettings.CommandLineArguments.AddRange(setVals);
                }

                await settingsProvider.WriteFrcSettingsAsync(currentSettings).ConfigureAwait(false);
            }

            return 0;
        }
    }
}
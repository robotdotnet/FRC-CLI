using System;
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

            SetupBaseOptions(command);
            
            command._teamOption.Description = 
                "Select team number to be set";

            return command;
        }

        public override async Task<int> RunAsync(string fileOrDirectory)
        {
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
                false, _verboseOption.HasValue());
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
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

                if (!await settingsProvider.WriteFrcSettingsAsync(currentSettings).ConfigureAwait(false))
                {
                    
                    throw scope.Resolve<IExceptionThrowerProvider>().ThrowException("Failed to write settings file"); 
                }
            }

            return 0;
        }
    }
}
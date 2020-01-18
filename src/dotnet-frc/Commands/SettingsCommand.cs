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
    internal class SettingsCommand : FrcSubCommandBase
    {
        private CommandOption _ignoreCommand;
        private CommandOption _argumentCommand;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private SettingsCommand()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {

        }

        public static DotNetSubCommandBase Create()
        {
            var command = new SettingsCommand
            {
                Name = "settings",
                Description = "Set FRC specific settings",
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
                false);
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                if (!_teamOption.HasValue() && !_ignoreCommand.HasValue()
                                           && !_argumentCommand.HasValue())
                {
                    throw scope.Resolve<IExceptionThrowerProvider>().ThrowException(
                        "No argument specified. Must provide an argument to use"
                    );
                }

                var settingsProvider = scope.Resolve<IFrcSettingsProvider>();
                FrcSettings? currentSettings = await settingsProvider.GetFrcSettingsAsync().ConfigureAwait(false);
                if (currentSettings == null)
                {
                    currentSettings = new FrcSettings("-1", new List<string>(), new List<string>());
                }

                if (_teamOption.HasValue())
                {
                    if (int.TryParse(_teamOption.Value(), out var teamNumber))
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
using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Common;
using FRC.CLI.Common.Connections;
using FRC.CLI.Common.Implementations;
using Microsoft.Build.Evaluation;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Utils;
using Nito.AsyncEx;

namespace dotnet_frc
{
    class KillCommand : DotNetSubCommandBase
    {
        private CommandOption _teamOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new KillCommand
            {
                Name = "kill",
                HandleRemainingArguments = false
            };

            command.HelpOption("-h|--help");
            
            command._teamOption = command.Option(
                "-t|--team",
                "Force a team",
                CommandOptionType.SingleValue
            );

            return command;
        }

        public async Task<int> RunAsync(string fileOrDirectory)
        {
            ConsoleWriter cWriter = new ConsoleWriter();

            int teamNumber = -1;
            bool parsed = false;
            if (_teamOption.HasValue())
            {
                parsed = int.TryParse(_teamOption.Value(), out teamNumber);
                if (!parsed)
                {
                    cWriter.WriteLine("Could not parse command line team number.");
                }
            }
            if (!parsed)
            {
                MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, fileOrDirectory);
                DotNetExceptionThrowerProvider exProvider = new DotNetExceptionThrowerProvider();
                DotNetProjectInformationProvider dnPjt = new DotNetProjectInformationProvider(msBuild);
                JsonFrcSettingsProvider frcSettingsProvider = new JsonFrcSettingsProvider(exProvider, cWriter, 
                    dnPjt);

                 var frcSettings = await frcSettingsProvider.GetFrcSettingsAsync().ConfigureAwait(false);
                if (frcSettings == null)
                {
                    throw exProvider.ThrowException("Could not find team number");
                }
                if (!int.TryParse(frcSettings.TeamNumber, out teamNumber))
                {
                    throw exProvider.ThrowException("Cannot parse team number from settings file");
                } 
            }
            using (RoboRioConnection rioConn = await RoboRioConnection.StartConnectionTaskAsync(teamNumber, cWriter))
            {
                if (!rioConn.Connected)
                {
                    throw new GracefulException("Could not connect to roboRio");
                }
                await rioConn.RunCommandsAsync(new string[] { DeployProperties.KillOnlyCommand}, ConnectionUser.LvUser);
            }

            return 0;
        }

        public override int Run(string fileOrDirectory)
        {
            return AsyncContext.Run(async () => await RunAsync(fileOrDirectory));
        }
    }
}
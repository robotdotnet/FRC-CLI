using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Utils;
using System.Threading.Tasks;
using FRC.CLI.Common.Connections;
using Nito.AsyncEx;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    internal class DeployCommand : DotNetSubCommandBase
    {
        private CommandOption _debugOption;
        private CommandOption _ipOption;
        private CommandOption _teamOption;

        public static DotNetSubCommandBase Create()
        {
            var command = new DeployCommand
            {
                Name = "deploy",
                HandleRemainingArguments = false
            };

            command.HelpOption("-h|--help");

            command._debugOption = command.Option(
                "-d|--debug <DEBUG>",
                "Debugger mode",
                CommandOptionType.NoValue
            );

            command._ipOption = command.Option(
                "-i|--ipaddr",
                "Force a deploy IP address",
                CommandOptionType.SingleValue
            );

            command._teamOption = command.Option(
                "-t|--team",
                "Force a team",
                CommandOptionType.SingleValue
            );

            return command;
        }
        public override int Run(string fileOrDirectory)
        {
            Console.WriteLine(fileOrDirectory);
            MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(new ProjectCollection(), fileOrDirectory);

            ISettingsProvider sProvider = new SettingsProvider();
            IOutputWriter cWriter = new ConsoleWriter();


            if (_teamOption.HasValue())
            {
                // Run a build, while attempting to connect
                var rioConn = AsyncContext.Run(async () =>
                {


                    var connectionTask = RoboRioConnection.StartConnectionTaskAsync(int.Parse(_teamOption.Value()),
                        cWriter, sProvider);

                    var buildTask = Task.Run(() =>
                    {
                        var cmdArgs = new List<string>
                        {
                            msBuild.ProjectFile,
                            "--configuration", "Release"
                        };

                        return Command.CreateDotNet("build", cmdArgs).Execute();
                        
                    });

                    await Task.WhenAll(connectionTask, buildTask).ConfigureAwait(false);

                    if (buildTask.Result.ExitCode != 0)
                    {
                        
                    }

                    return new Tuple<RoboRioConnection, int>(connectionTask.Result, buildTask.Result.ExitCode);
                });

                if (rioConn.Item2 != 0)
                {
                    throw new GracefulException("Could not build robot code");
                }

                if (!rioConn.Item1.Connected)
                {
                    throw new GracefulException("Cannot connect to rio");
                }

                Console.WriteLine("Connected and built successfully!!!");
            }
            
            // IP over team

            return 0;
        }
    }
}
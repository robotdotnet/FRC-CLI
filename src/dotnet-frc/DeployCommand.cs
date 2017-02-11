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
using FRC.CLI.Common;
using FRC.CLI.Common.Implementations;

namespace dotnet_frc
{
    internal class DeployCommand : DotNetSubCommandBase
    {
        private CommandOption _debugOption;
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

            command._teamOption = command.Option(
                "-t|--team",
                "Force a team",
                CommandOptionType.SingleValue
            );

            return command;
        }

        public async Task RunDeployAsync(MsBuildProject buildProject)
        {
            DotNetExceptionThrowerProvider dnExProvider = new DotNetExceptionThrowerProvider();
            DotNetProjectInformationProvider dnPjtProvider = new DotNetProjectInformationProvider(buildProject);
            DotNetBuildSettingsProvider dnBsProvider = new DotNetBuildSettingsProvider(_debugOption.HasValue());
            DotNetCodeBuilder dnCodeBuilder = new DotNetCodeBuilder(dnPjtProvider, dnBsProvider, dnExProvider);
            WPILibNativeDeploySettingsProvider wpiNativeDeployProvider = new WPILibNativeDeploySettingsProvider();
            
            NativePackageDeploymentProvider nativeDeploymentProvider = new NativePackageDeploymentProvider(wpiNativeDeployProvider, dnPjtProvider, dnExProvider);

            WPILibImageSettingsProvider wpiImageSetProvider = new WPILibImageSettingsProvider();

            RoboRioImageProvider rioImageProvider = new RoboRioImageProvider(wpiImageSetProvider, dnExProvider);

            ConsoleWriter cWriter = new ConsoleWriter();

            JsonFrcSettingsProvider frcSettingsProvider = new JsonFrcSettingsProvider(dnExProvider, cWriter, dnPjtProvider);

            RoboRioDependencyCheckerProvider rioDepProvider = new RoboRioDependencyCheckerProvider();

            RobotCodeDeploymentProvider codeDeployProvider = new RobotCodeDeploymentProvider(cWriter, dnPjtProvider, nativeDeploymentProvider, dnBsProvider, frcSettingsProvider);

            CodeDeployer deployer = new CodeDeployer(dnCodeBuilder, dnExProvider, rioImageProvider, cWriter, rioDepProvider, codeDeployProvider, nativeDeploymentProvider);

            int teamNumber = -1;
            bool parsed = false;
            if (_teamOption.HasValue())
            {
                parsed = int.TryParse(_teamOption.Value(), out teamNumber);
                if (!parsed)
                {
                    cWriter.WriteLine("Could not parse command line team number. Attempting to use settings");
                }
            }
            if (!parsed)
            {
                var frcSettings = await frcSettingsProvider.GetFrcSettingsAsync().ConfigureAwait(false);
                if (frcSettings == null)
                {
                    throw dnExProvider.ThrowException("Could not find team number");
                }
                if (!int.TryParse(frcSettings.TeamNumber, out teamNumber))
                {
                    throw dnExProvider.ThrowException("Cannot parse team number from settings file");
                }
            }
            
            using (RoboRioConnection rioConn = await RoboRioConnection.StartConnectionTaskAsync(teamNumber, cWriter))
            {
                if (!rioConn.Connected)
                {
                    throw dnExProvider.ThrowException("Could not connect to roboRio");
                }
                await deployer.DeployCode(rioConn);
            }
        }

        public override int Run(string fileOrDirectory)
        {
            MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, fileOrDirectory);

            if (!msBuild.GetIsWPILibProject())
            {
                throw new GracefulException("Detected project is not a WPILib project");
            }

            AsyncContext.Run(async () => await RunDeployAsync(msBuild));

            

            /*

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
                            "--configuration", "Release",
                            "-o", "frctemp"
                        };

                        return Command.CreateDotNet("build", cmdArgs).Execute();
                        
                    });

                    await Task.WhenAll(connectionTask, buildTask).ConfigureAwait(false);

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

            */

            return 0;
        }
    }
}
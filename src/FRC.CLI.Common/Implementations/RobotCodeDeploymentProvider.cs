using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Base.Models;

namespace FRC.CLI.Common.Implementations
{
    public class RobotCodeDeploymentProvider : IRobotCodeDeploymentProvider
    {
        private IOutputWriter m_outputWriter;
        private IProjectInformationProvider m_projectInformationProvider;
        private INativeContentDeploymentProvider m_nativePackageDeploymentProvider;
        private IBuildSettingsProvider m_buildSettingsProvider;
        private IFrcSettingsProvider m_frcSettingsProvider;
        private IFileDeployerProvider m_fileDeployerProvider;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;

        public RobotCodeDeploymentProvider(IOutputWriter outputWriter,
            IProjectInformationProvider projectInformationProvider,
            INativeContentDeploymentProvider nativePackageDeploymentProvider,
            IBuildSettingsProvider buildSettingsProvider,
            IFrcSettingsProvider frcSettingsProvider,
            IFileDeployerProvider fileDeployerProvider,
            IExceptionThrowerProvider exceptionThrowerProvider)
        {
            m_outputWriter = outputWriter;
            m_projectInformationProvider = projectInformationProvider;
            m_nativePackageDeploymentProvider = nativePackageDeploymentProvider;
            m_buildSettingsProvider = buildSettingsProvider;
            m_frcSettingsProvider = frcSettingsProvider;
            m_fileDeployerProvider = fileDeployerProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
        }

        private async Task EnsureRemoteDirectoryExists(string directory)
        {
            bool verbose = m_buildSettingsProvider.Verbose;
            if (verbose)
            {
                await m_outputWriter.WriteLineAsync("Creating Code Deploy Directory").ConfigureAwait(false);
            }
            // Ensure output directory exists
            await m_fileDeployerProvider.RunCommandAsync($"mkdir -p {DeployProperties.DeployDir}", ConnectionUser.LvUser).ConfigureAwait(false);
        }

        public virtual IEnumerable<string> GetListOfFilesToDeploy(IEnumerable<string> allFiles,
            FrcSettings frcSettings, IEnumerable<string> ignoreDirectories,
            IEnumerable<string> ignoreFiles)
        {
            List<string> settingsIgnoreFiles = frcSettings?.DeployIgnoreFiles ?? new List<string>();
            return allFiles.Where(x => !ignoreDirectories.Any(x.Contains))
                           .Where(x => !ignoreFiles.Any(x.Contains))
                           .Where(x => !settingsIgnoreFiles.Any(x.Contains));

        }

        public async Task DeployRobotCodeAsync()
        {
            await m_outputWriter.WriteLineAsync("Deploying Robot Code Files").ConfigureAwait(false);
            bool verbose = m_buildSettingsProvider.Verbose;
            string buildDir = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);
            string nativeDir = Path.Combine(buildDir, m_nativePackageDeploymentProvider.NativeDirectory);
            if (verbose)
            {
                await m_outputWriter.WriteLineAsync("Creating Mono Deploy Directory").ConfigureAwait(false);
            }
            // Ensure output directory exists
            await m_fileDeployerProvider.RunCommandAsync($"mkdir -p {DeployProperties.DeployDir}", ConnectionUser.LvUser).ConfigureAwait(false);
            // Ignore User specified ignore files, and the wpinative folder
            List<string> ignoreFiles = (await m_frcSettingsProvider.GetFrcSettingsAsync()
                .ConfigureAwait(false))
                ?.DeployIgnoreFiles;
            var files = Directory.GetFiles(buildDir, "*", SearchOption.AllDirectories).Where(x => !x.Contains(nativeDir))
                                                    .Where(f => !DeployProperties.IgnoreFiles.Any(f.Contains))
                                                    .Where(f => !ignoreFiles.Any(f.Contains))
                                                    .Select(x =>
                                                    {
                                                        var path = Path.GetDirectoryName(x);
                                                        string split = path.Substring(buildDir.Length);
                                                        split = split.Replace('\\', '/');
                                                        return (x, $"{DeployProperties.DeployDir}{split}");
                                                    });
            // Deploy all files
            if (!await m_fileDeployerProvider.DeployFilesAsync(files, ConnectionUser.LvUser).ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to deploy robot files");
            }
            await m_outputWriter.WriteLineAsync("Successfully deployed robot code files").ConfigureAwait(false);
        }

        public async Task StartRobotCodeAsync()
        {
            await m_outputWriter.WriteLineAsync("Starting robot code").ConfigureAwait(false);
            bool debug = m_buildSettingsProvider.Debug;
            // Force release until I can get debugging working properly
            debug = false;

            // Run kill command to ensure that no code issues happen
            // Ignore output, as this failing is not a big issue.
            await m_fileDeployerProvider.RunCommandAsync(DeployProperties.KillOnlyCommand, ConnectionUser.LvUser).ConfigureAwait(false);

            //Combining all other commands, since they should be safe running together.
            List<string> commands = new List<string>();
            string robotName = await m_projectInformationProvider.GetProjectExecutableNameAsync().ConfigureAwait(false);

            string deployedCmd;
            string deployedCmdFrame;

            string ipAddress = (await m_fileDeployerProvider.GetConnectionIpAsync().ConfigureAwait(false))?.ToString();

            if (debug)
            {
                deployedCmd = string.Format(DeployProperties.RobotCommandDebug, robotName, ipAddress);
                deployedCmdFrame = DeployProperties.RobotCommandDebugFileName;
            }
            else
            {
                deployedCmd = string.Format(DeployProperties.RobotCommand, robotName);
                deployedCmdFrame = DeployProperties.RobotCommandFileName;
            }

            List<string> requestedArguments = (await m_frcSettingsProvider.GetFrcSettingsAsync()
                .ConfigureAwait(false))
                ?.CommandLineArguments;
            string args = "";
            if (requestedArguments != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach(var arg in requestedArguments)
                {
                    builder.Append(arg);
                    builder.Append(" ");
                }
                args = builder.ToString();
            }

            //Write the robotCommand file
            commands.Add($"echo {deployedCmd} {args} > {DeployProperties.CommandDir}/{deployedCmdFrame}");
            if (debug)
            {
                //If debug write the debug flag.
                commands.AddRange(DeployProperties.DebugFlagCommand);
            }
            // Run a sync to ensure all files get saved
            commands.Add("sync");
            //run all commands
            await m_fileDeployerProvider.RunCommandsAsync(commands.ToArray(), ConnectionUser.LvUser).ConfigureAwait(false);

            //Run start command individually so we can chec to make sure everything works.
            var codeRet = await m_fileDeployerProvider.RunCommandAsync(DeployProperties.DeployKillCommand,
                ConnectionUser.LvUser).ConfigureAwait(false);
            if (codeRet.ExitStatus != 0)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to successfully start robot code");
            }
            await m_outputWriter.WriteLineAsync("Successfully started code").ConfigureAwait(false);
        }
    }
}
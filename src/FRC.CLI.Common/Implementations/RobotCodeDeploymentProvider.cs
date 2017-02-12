using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class RobotCodeDeploymentProvider : IRobotCodeDeploymentProvider
    {
        IOutputWriter m_outputWriter;
        IProjectInformationProvider m_projectInformationProvider;
        INativePackageDeploymentProvider m_nativePackageDeploymentProvider;
        IBuildSettingsProvider m_buildSettingsProvider;
        IFrcSettingsProvider m_frcSettingsProvider;
        IFileDeployerProvider m_fileDeployerProvider;

        public RobotCodeDeploymentProvider(IOutputWriter outputWriter,
            IProjectInformationProvider projectInformationProvider,
            INativePackageDeploymentProvider nativePackageDeploymentProvider,
            IBuildSettingsProvider buildSettingsProvider,
            IFrcSettingsProvider frcSettingsProvider,
            IFileDeployerProvider fileDeployerProvider)
        {
            m_outputWriter = outputWriter;
            m_projectInformationProvider = projectInformationProvider;
            m_nativePackageDeploymentProvider = nativePackageDeploymentProvider;
            m_buildSettingsProvider = buildSettingsProvider;
            m_frcSettingsProvider = frcSettingsProvider;
            m_fileDeployerProvider = fileDeployerProvider;
        }

        public async Task<bool> DeployRobotCodeAsync()
        {
            
            
            string buildDir = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);
            string nativeDir = Path.Combine(buildDir, m_nativePackageDeploymentProvider.NativeDirectory);
            await m_outputWriter.WriteLineAsync("Creating Mono Deploy Directory").ConfigureAwait(false);
            await m_fileDeployerProvider.RunCommandsAsync(new string[] {$"mkdir -p {DeployProperties.DeployDir}"}, ConnectionUser.LvUser).ConfigureAwait(false);
            var files = Directory.GetFiles(buildDir).Where(x => !x.Contains(nativeDir)).Where(f => !DeployProperties.IgnoreFiles.Any(f.Contains));
            await m_outputWriter.WriteLineAsync("Deploying robot files").ConfigureAwait(false);
            return await m_fileDeployerProvider.DeployFilesAsync(files, DeployProperties.DeployDir, ConnectionUser.LvUser);
        }

        public async Task<bool> StartRobotCodeAsync()
        {
            bool debug = m_buildSettingsProvider.Debug;

            await m_fileDeployerProvider.RunCommandsAsync(new string[] {DeployProperties.KillOnlyCommand}, ConnectionUser.LvUser).ConfigureAwait(false);

            //Combining all other commands, since they should be safe running together.
            List<string> commands = new List<string>();
            string robotName = await m_projectInformationProvider.GetProjectExecutableNameAsync().ConfigureAwait(false);
            
            string deployedCmd;
            string deployedCmdFrame;

            if (debug)
            {
                deployedCmd = string.Format(DeployProperties.RobotCommandDebug, robotName);
                deployedCmdFrame = DeployProperties.RobotCommandDebugFileName;
            }
            else
            {
                deployedCmd = string.Format(DeployProperties.RobotCommand, robotName);
                deployedCmdFrame = DeployProperties.RobotCommandFileName;
            }

            //Write the robotCommand file
            commands.Add($"echo {deployedCmd} > {DeployProperties.CommandDir}/{deployedCmdFrame}");
            if (debug)
            {
                //If debug write the debug flag.
                commands.AddRange(DeployProperties.DebugFlagCommand);
            }
            //Add all commands to restart
            commands.AddRange(DeployProperties.DeployKillCommand);
            //run all commands
            await m_fileDeployerProvider.RunCommandsAsync(commands.ToArray(), ConnectionUser.LvUser).ConfigureAwait(false);

            //Run sync so files are written to disk.
            await m_fileDeployerProvider.RunCommandsAsync(new string[] {"sync"}, ConnectionUser.LvUser).ConfigureAwait(false);
            // TODO: Figure out when to make this return false;
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class RobotCodeDeploymentProvider : IRobotCodeDeploymentProvider
    {
        IOutputWriter m_outputWriter;

        public RobotCodeDeploymentProvider(IOutputWriter outputWriter)
        {
            m_outputWriter = outputWriter;
        }

        public async Task<bool> DeployRobotCodeAsync(string localCodeLocation, IFileDeployerProvider fileDeployerProvider)
        {
            
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(localCodeLocation));
            await m_outputWriter.WriteLineAsync("Creating Mono Deploy Directory").ConfigureAwait(false);
            await fileDeployerProvider.RunCommandsAsync(new string[] {$"mkdir -p /home/lvuser/mono"}, ConnectionUser.LvUser).ConfigureAwait(false);
            await m_outputWriter.WriteLineAsync("Deploying robot files").ConfigureAwait(false);
            return await fileDeployerProvider.DeployFilesAsync(files, "/home/lvuser/mono", ConnectionUser.LvUser);
        }

        public async Task<bool> StartRobotCodeAsync(string executableName, IFileDeployerProvider fileDeployerProvider)
        {
            bool debug = false;

            await fileDeployerProvider.RunCommandsAsync(new string[] {". /etc/profile.d/natinst-path.sh; /usr/local/frc/bin/frcKillRobot.sh -t"}, ConnectionUser.LvUser).ConfigureAwait(false);

            //Combining all other commands, since they should be safe running together.
            List<string> commands = new List<string>();

            string deployedCmd = string.Format("env LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/frc/lib mono \"" + "/home/lvuser/mono" + "/{0}\"", executableName);
            string deployedCmdFrame = "robotCommand";

            //Write the robotCommand file
            commands.Add($"echo {deployedCmd} > /home/lvuser/{deployedCmdFrame}");
            //Add all commands to restart
            commands.Add(". /etc/profile.d/natinst-path.sh; /usr/local/frc/bin/frcKillRobot.sh -t -r;");
            //run all commands
            await fileDeployerProvider.RunCommandsAsync(commands.ToArray(), ConnectionUser.LvUser).ConfigureAwait(false);

            //Run sync so files are written to disk.
            await fileDeployerProvider.RunCommandsAsync(new string[] {"sync"}, ConnectionUser.LvUser).ConfigureAwait(false);
            return true;
        }
    }
}
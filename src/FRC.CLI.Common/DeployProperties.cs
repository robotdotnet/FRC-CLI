namespace FRC.CLI.Common
{
    public static class DeployProperties
    {
        public const string UserName = "lvuser";
        public const string DeployDir = "/home/lvuser/mono";
        public const string DeployKillCommand =
            ". /etc/profile.d/natinst-path.sh; /usr/local/frc/bin/frcKillRobot.sh -t -r;";
            
        public const string UserLibraryDir = "/usr/local/frc/lib";

        public const string KillOnlyCommand = ". /etc/profile.d/natinst-path.sh; /usr/local/frc/bin/frcKillRobot.sh -t";

        public const string DebugFlagDir = "/tmp/";

        public static readonly string[] DebugFlagCommand =
        {
            "touch /tmp/frcdebug",
            $"chown lvuser:ni {DebugFlagDir}frcdebug"
        };

        public const string RoboRioMonoBin = "/usr/bin/mono";

        public static readonly string[] IgnoreFiles =
        {
            ".vshost",
            ".config",
            ".manifest",
            "FRC.NetworkTables.Core.DesktopLibraries.dll",
            "FRC.OpenCvSharp.DesktopLibraries.dll",
            "FRC.HAL.DesktopLibraries.dll"
        };

        public const string CommandDir = "/home/lvuser";

        public const string RobotCommandDebug = "env LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/frc/lib mono"
            + " --debug --debugger-agent=transport=dt_socket,server=y,address={1}:55555 \"" + DeployDir + "/{0}\"";
            //--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555
        public const string RobotCommandDebugFileName = "robotDebugCommand";

        public const string RobotCommand = "env LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/frc/lib mono \"" + DeployDir + "/{0}\"";

        public const string RobotCommandFileName = "robotCommand";

        public const string MonoMd5 = "1852022171552091945152452461853193134197150";

        public const string MonoVersion = "Mono4.2.1.zip";

        public const string MonoUrl = "https://dl.bintray.com/robotdotnet/Mono/";
    }
}
namespace FRC.CLI.Common
{
    public static class DeployProperties
    {
        public const string UserName = "lvuser";
        public const string DeployDir = "/home/lvuser/dotnet";
        public const string DeployKillCommand =
            ". /etc/profile.d/natinst-path.sh; /usr/local/frc/bin/frcKillRobot.sh -t -r;";

        public const string UserLibraryDir = "/usr/local/frc/third-party/lib";

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
        };

        public const string CommandDir = "/home/lvuser";

        public const string RobotCommandDebug = "env LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/frc/lib mono"
            + " --debug --debugger-agent=transport=dt_socket,server=y,address={1}:55555 \"" + DeployDir + "/{0}\"";
            //--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555
        public const string RobotCommandDebugFileName = "robotDebugCommand";

        public const string RobotCommand = "env MONO_THREADS_SUSPEND=coop mono \"" + DeployDir + "/{0}\"";

        public const string RobotCommandFileName = "robotCommand";

        public const string MonoMd5 = "769F3315B11F2A78A33EAFDC50FB2410";

        public const string MonoZipName = "Mono6.8.0.96-2020-0.zip";

        public const string MonoUrl = "https://github.com/robotdotnet/Mono-Releases/releases/download/v6.8.0.96-2020-0/Mono6.8.0.96-2020-0.zip";
    }
}
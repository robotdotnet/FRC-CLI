using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class DotNetBuildSettingsProvider : IBuildSettingsProvider
    {
        public bool Debug { get; }

        public DotNetBuildSettingsProvider(bool debug)
        {
            Debug = debug;
        }
    }
}
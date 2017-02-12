using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class DotNetBuildSettingsProvider : IBuildSettingsProvider
    {
        public bool Debug { get; }
        public bool Verbose { get; }

        public DotNetBuildSettingsProvider(bool debug, bool verbose)
        {
            Debug = debug;
            Verbose = verbose;
        }
    }
}
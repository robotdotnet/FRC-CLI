namespace FRC.CLI.Base.Interfaces
{
    public interface ISettingsProvider
    {
        bool Verbose { get; }

        bool DebugMode { get; }
    }
}
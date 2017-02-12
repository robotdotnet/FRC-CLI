namespace FRC.CLI.Base.Interfaces
{
    public interface IBuildSettingsProvider
    {
         bool Debug { get; }
         bool Verbose { get; }
    }
}
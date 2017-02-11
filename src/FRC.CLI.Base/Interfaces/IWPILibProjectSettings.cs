namespace FRC.CLI.Base.Interfaces
{
    public interface IWPILibProjectSettings
    {
         bool IsWPILibProject { get; }
         int? TeamNumber { get; }

         string ProjectFileLocation { get; }
    }
}
namespace FRC.CLI.Base.Interfaces
{
    public interface IMonoFileConstantsProvider
    {
         string Url { get; }
         string OutputFileName { get; }
         string Md5Sum { get; }
    }
}
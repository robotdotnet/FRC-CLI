using Microsoft.DotNet.Cli.Utils;


namespace dotnet_frc
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);
            return FrcCommand.Run(args);
        }
    }
}

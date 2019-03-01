using System.Threading.Tasks;
using Microsoft.DotNet.Cli.Utils;


namespace dotnet_frc
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);
            return FrcCommand.RunAsync(args);
        }
    }
}

using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class DotNetNativeContentLocalLocationProvider : INativeContentLocalLocationProvider
    {
        public Task<string> GetNativeContentLocationAsync()
        {
            return Task.FromResult(@"C:\Users\thadh\.nuget\packages\FRC.WPILibNativeLibraries\2017.0.5\content\wpinative");
        }
    }
}
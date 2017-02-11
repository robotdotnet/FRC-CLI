using System.Collections.Generic;

namespace FRC.CLI.Base.Interfaces
{
    public interface IDeviceSettingsProvider
    {
         string NativeDeployLocation { get; }
         string NativePropertiesFileName { get; }
         List<string> NativeIgnoreFiles { get; }
         string RequiredImageVersion { get; }
    }
}
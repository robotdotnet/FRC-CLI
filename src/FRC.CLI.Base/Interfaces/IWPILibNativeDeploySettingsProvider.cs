using System.Collections.Generic;

namespace FRC.CLI.Base.Interfaces
{
    public interface IWPILibNativeDeploySettingsProvider
    {
         string NativeDeployLocation { get; }
         string NativePropertiesFileName { get; }
         IList<string> NativeIgnoreFiles { get; }
    }
}
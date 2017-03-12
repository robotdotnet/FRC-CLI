using System.Collections.Generic;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class WPILibNativeDeploySettingsProvider : IWPILibNativeDeploySettingsProvider
    {
        public string NativeDeployLocation =>  "/usr/local/frc/lib";

        public string NativePropertiesFileName => "WPI_Native_Libraries.properties";

        public IList<string> NativeIgnoreFiles
        {
            get
            {
                return new List<string>
                {
                    "ImageSettings.json"
                };
            }
        }
    }
}
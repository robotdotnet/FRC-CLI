using System.Collections.Generic;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class WPILibImageSettingsProvider : IWPILibImageSettingsProvider
    {
        public Task<IList<string>> GetAllowedImageVersionsAsync()
        {
            IList<string> result = new List<string>
            {
                "FRC_roboRIO_2017_v8"
            };
            return Task.FromResult(result);
        }
    }
}
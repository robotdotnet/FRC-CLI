using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IWPILibImageSettingsProvider
    {
         Task<IList<string>> GetAllowedImageVersionsAsync();
    }
}
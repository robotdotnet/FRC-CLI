using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRoboRioImageProvider
    {
        Task CheckCorrectImageAsync();

        Task<string> GetCurrentRoboRioImageAsync();

        Task<IList<string>> GetAllowedRoboRioImagesAsync();
    }
}
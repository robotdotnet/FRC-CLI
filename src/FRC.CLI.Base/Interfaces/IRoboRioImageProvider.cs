using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRoboRioImageProvider
    {
        Task<bool> CheckCorrectImageAsync();

        Task<int> GetCurrentRoboRioImageAsync();

        Task<List<int>> GetAllowedRoboRioImagesAsync();
    }
}
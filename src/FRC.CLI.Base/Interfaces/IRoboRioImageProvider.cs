using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRoboRioImageProvider
    {
        Task<bool> CheckCorrectImageAsync(IFileDeployerProvider fileDeployerProvider);

        Task<string> GetCurrentRoboRioImageAsync(IFileDeployerProvider fileDeployerProvider);

        Task<IList<string>> GetAllowedRoboRioImagesAsync();
    }
}
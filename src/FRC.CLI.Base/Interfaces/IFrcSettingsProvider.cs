using System.Threading.Tasks;
using FRC.CLI.Base.Models;

namespace FRC.CLI.Base.Interfaces
{
    public interface IFrcSettingsProvider
    {
        Task<FrcSettings> GetFrcSettingsAsync();
        Task<bool> WriteFrcSettingsAsync(FrcSettings settings);
    }
}
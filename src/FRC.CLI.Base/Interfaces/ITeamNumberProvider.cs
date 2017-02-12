using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface ITeamNumberProvider
    {
        Task<int> GetTeamNumberAsync();
    }
}
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IRoboRioDependencyCheckerProvider
    {
         Task CheckIfDependenciesAreSatisfiedAsync();
    }
}
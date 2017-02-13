using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioDependencyCheckerProvider : IRoboRioDependencyCheckerProvider
    {
        private IMonoInstallCheckerProvider m_monoInstallCheckerProvider;
        public RoboRioDependencyCheckerProvider(IMonoInstallCheckerProvider monoInstallCheckerProvider)
        {
            m_monoInstallCheckerProvider = monoInstallCheckerProvider;
        }

        public async Task<bool> CheckIfDependenciesAreSatisfied()
        {
            // Check mono install
            return await m_monoInstallCheckerProvider.CheckMonoInstallAsync().ConfigureAwait(false);
        }
    }
}
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioDependencyCheckerProvider : IRoboRioDependencyCheckerProvider
    {
        private IRuntimeProvider m_runtimeProvider;
        private IOutputWriter m_outputWriter;

        public RoboRioDependencyCheckerProvider(IRuntimeProvider runtimeProvider,
            IOutputWriter outputWriter)
        {
            m_runtimeProvider = runtimeProvider;
            m_outputWriter = outputWriter;
        }

        public async Task CheckIfDependenciesAreSatisfiedAsync()
        {
            await m_outputWriter.WriteLineAsync("Checking dependencies").ConfigureAwait(false);
            // Check mono install
            await m_runtimeProvider.VerifyRuntimeAsync().ConfigureAwait(false);

            await m_outputWriter.WriteLineAsync("All dependencies satisfied").ConfigureAwait(false);
        }
    }
}
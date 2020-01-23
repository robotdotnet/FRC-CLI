using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common
{
    public class CodeDeployer
    {
        private readonly ICodeBuilderProvider m_codeBuilderProvider;
        private readonly IExceptionThrowerProvider m_exceptionThrowerProvider;
        private readonly IRoboRioImageProvider m_roboRioImageProvider;
        private readonly IOutputWriter m_outputWriter;
        private readonly IRoboRioDependencyCheckerProvider m_roboRioDependencyCheckerProvider;
        private readonly IRobotCodeDeploymentProvider m_robotCodeDeploymentProvider;
        private readonly INativeContentDeploymentProvider m_nativePackageDeploymentProvider;

        public CodeDeployer(ICodeBuilderProvider codeBuilderProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IRoboRioImageProvider roboRioImageProvider, IOutputWriter outputWriter,
            IRoboRioDependencyCheckerProvider roboRioDependencyCheckerProvider,
            IRobotCodeDeploymentProvider robotCodeDeploymentProvider,
            INativeContentDeploymentProvider nativePackageDeploymentProvider)
        {
            m_codeBuilderProvider = codeBuilderProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_roboRioImageProvider = roboRioImageProvider;
            m_outputWriter = outputWriter;
            m_roboRioDependencyCheckerProvider = roboRioDependencyCheckerProvider;
            m_robotCodeDeploymentProvider = robotCodeDeploymentProvider;
            m_nativePackageDeploymentProvider = nativePackageDeploymentProvider;
        }

        public async Task DeployCode()
        {
            // Build code
            await m_codeBuilderProvider.BuildCodeAsync().ConfigureAwait(false);

            // Check image
            //await m_roboRioImageProvider.CheckCorrectImageAsync().ConfigureAwait(false);

            //await m_roboRioDependencyCheckerProvider.CheckIfDependenciesAreSatisfiedAsync().ConfigureAwait(false);

            await m_nativePackageDeploymentProvider.DeployNativeContentAsync().ConfigureAwait(false);

            // Deploy robot code
            await m_robotCodeDeploymentProvider.DeployRobotCodeAsync().ConfigureAwait(false);

            // Start robot code
            await m_robotCodeDeploymentProvider.StartRobotCodeAsync().ConfigureAwait(false);
        }
    }
}
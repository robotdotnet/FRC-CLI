using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common
{
    public class CodeDeployer
    {
        private ICodeBuilderProvider m_codeBuilderProvider;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;
        private IRoboRioImageProvider m_roboRioImageProvider;
        private IOutputWriter m_outputWriter;
        private IRoboRioDependencyCheckerProvider m_roboRioDependencyCheckerProvider;
        private IRobotCodeDeploymentProvider m_robotCodeDeploymentProvider;
        private INativePackageDeploymentProvider m_nativePackageDeploymentProvider;

        public CodeDeployer(ICodeBuilderProvider codeBuilderProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IRoboRioImageProvider roboRioImageProvider, IOutputWriter outputWriter,
            IRoboRioDependencyCheckerProvider roboRioDependencyCheckerProvider,
            IRobotCodeDeploymentProvider robotCodeDeploymentProvider,
            INativePackageDeploymentProvider nativePackageDeploymentProvider)
        {
            m_codeBuilderProvider = codeBuilderProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_roboRioImageProvider = roboRioImageProvider;
            m_outputWriter = outputWriter;
            m_roboRioDependencyCheckerProvider = roboRioDependencyCheckerProvider;
            m_robotCodeDeploymentProvider = robotCodeDeploymentProvider;
            m_nativePackageDeploymentProvider = nativePackageDeploymentProvider;
        }
        

        public async Task DeployCode(IFileDeployerProvider fileDeployerProvider)
        {                
            // Build code
            var buildResult = await m_codeBuilderProvider.BuildCodeAsync().ConfigureAwait(false);
            if (buildResult.Item1 != 0)
            {
                throw m_exceptionThrowerProvider.ThrowException("Could not successfully build code");
            }

            // Check image
            if (!await m_roboRioImageProvider.CheckCorrectImageAsync(fileDeployerProvider).ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("RoboRio image is not correct");
            }

            if (!await m_roboRioDependencyCheckerProvider.CheckIfDependenciesAreSatisfied().ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("Native Dependency requirements are not satisfied");
            }


            bool nativeDeployedDeploy = await m_nativePackageDeploymentProvider.DeployNativeFilesAsync(fileDeployerProvider).ConfigureAwait(false);

            // Deploy robot code
            if (!await m_robotCodeDeploymentProvider.DeployRobotCodeAsync(buildResult.Item2, fileDeployerProvider).ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("Could not deploy robot code");
            }

            // Start robot code
            if (!await m_robotCodeDeploymentProvider.StartRobotCodeAsync(buildResult.Item3, fileDeployerProvider).ConfigureAwait(false))
            {
                throw m_exceptionThrowerProvider.ThrowException("Could not property start robot code");
            }

            await m_outputWriter.WriteLineAsync("Successfully deployed and started code").ConfigureAwait(false);
        }
    }
}
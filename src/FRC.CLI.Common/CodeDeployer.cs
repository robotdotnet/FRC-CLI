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

        public CodeDeployer(ICodeBuilderProvider codeBuilderProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IRoboRioImageProvider roboRioImageProvider, IOutputWriter outputWriter,
            IRoboRioDependencyCheckerProvider roboRioDependencyCheckerProvider,
            IRobotCodeDeploymentProvider robotCodeDeploymentProvider)
        {
            m_codeBuilderProvider = codeBuilderProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_roboRioImageProvider = roboRioImageProvider;
            m_outputWriter = outputWriter;
            m_roboRioDependencyCheckerProvider = roboRioDependencyCheckerProvider;
            m_robotCodeDeploymentProvider = robotCodeDeploymentProvider;
        }
        

        public async Task DeployCode()
        {                
            // Build code
            var buildResult = await m_codeBuilder.BuildCodeAsync();
            if (buildResult.Item1 != 0)
            {
                throw exceptionThrower.ThrowException("Could not successfully build code");
            }

            // Check image
            if (!await rioImageProvider.CheckCorrectImageAsync())
            {
                throw exceptionThrower.ThrowException("RoboRio image is not correct");
            }

            if (!await roboRioDependencyChecker.CheckIfDependenciesAreSatisfied())
            {
                throw exceptionThrower.ThrowException("Native Dependency requirements are not satisfied");
            }


            bool nativeDeployResult = await nativeProvider.DeployNativeFilesAsync();

            if (!nativeDeployResult)
            {
                throw exceptionThrower.ThrowException("Could not properly deploy native files");
            }

            // Deploy robot code
            if (!await robotDeploymentProvider.DeployRobotCodeAsync())
            {
                throw exceptionThrower.ThrowException("Could not deploy robot code");
            }

            // Start robot code
            if (!await robotDeploymentProvider.StartRobotCodeAsync())
            {
                throw exceptionThrower.ThrowException("Could not property start robot code");
            }

            await outputWriter.WriteLineAsync("Successfully deployed and started code");
        }
    }
}
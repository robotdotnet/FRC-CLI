using System.Collections.Generic;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using Microsoft.DotNet.Cli.Utils;

namespace dotnet_frc
{
    public class DotNetCodeBuilder : ICodeBuilderProvider
    {
        private IProjectInformationProvider m_projectInformationProvider;
        private IBuildSettingsProvider m_buildSettingsProvider;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;
        private IOutputWriter m_outputWriter;

        public DotNetCodeBuilder(IProjectInformationProvider projectInformationProvider, IBuildSettingsProvider buildSettingsProvider,
            IExceptionThrowerProvider exceptionThrowerProvider, IOutputWriter outputWriter)
        {
            m_projectInformationProvider = projectInformationProvider;
            m_buildSettingsProvider = buildSettingsProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_outputWriter = outputWriter;
        }

        public async Task BuildCodeAsync()
        {
            await m_outputWriter.WriteLineAsync("Starting code build").ConfigureAwait(false);
            string outputLoc = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);
            string projectDir = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            var retVal = await Task.Run(() =>
            {
                string configuration = "Release";
                if (m_buildSettingsProvider.Debug)
                {
                    configuration = "Debug";
                }
                var cmdArgs = new List<string>
                {
                    projectDir,
                    "--configuration", configuration,
                    "-o", outputLoc
                };

                var result = Command.CreateDotNet("build", cmdArgs).Execute();
                return result;
            }).ConfigureAwait(false);
            if (retVal.ExitCode != 0)
            {
                throw m_exceptionThrowerProvider.ThrowException($"Failed to build code. Error code: {retVal.ExitCode}");
            } 
            await m_outputWriter.WriteLineAsync("Successfully built code").ConfigureAwait(false);
        }
    }
}
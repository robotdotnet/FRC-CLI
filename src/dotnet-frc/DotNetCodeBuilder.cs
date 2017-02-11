using System;
using System.Collections.Generic;
using System.IO;
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

        public DotNetCodeBuilder(IProjectInformationProvider projectInformationProvider, IBuildSettingsProvider buildSettingsProvider,
            IExceptionThrowerProvider exceptionThrowerProvider)
        {
            m_projectInformationProvider = projectInformationProvider;
            m_buildSettingsProvider = buildSettingsProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
        }

        public async Task<bool> BuildCodeAsync()
        {
            string outputLoc = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);
            string projectDir = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            int retVal = await Task.Run(() =>
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
                return result.ExitCode;
            }).ConfigureAwait(false);
            return retVal == 0;
        }
    }
}
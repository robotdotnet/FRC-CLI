using System;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class DotNetProjectInformationProvider : IProjectInformationProvider
    {
        private string m_projectRoot;
        private string m_buildDirectory;
        private string m_executableName;
        public DotNetProjectInformationProvider(MsBuildProject msBuild)
        {
            m_projectRoot = msBuild.ProjectDirectory;
            m_buildDirectory = Path.Combine(m_projectRoot, "bin", "frctemp");
            m_executableName = msBuild.GetProjectAssemblyName();
        }

        public Task<string> GetProjectBuildDirectoryAsync()
        {
            return Task.FromResult(m_buildDirectory);
        }

        public Task<string> GetProjectRootDirectoryAsync()
        {
            return Task.FromResult(m_projectRoot);
        }

        public Task<string> GetProjectExecutableNameAsync()
        {
            return Task.FromResult(m_executableName);
        }
    }
}
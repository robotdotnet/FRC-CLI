using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class MockProjectInformationProvider : IProjectInformationProvider
    {
        public MockProjectInformationProvider()
        {
        }

        public Task<string> GetProjectBuildDirectoryAsync()
        {
            return Task.FromResult("");
        }

        public Task<string> GetProjectRootDirectoryAsync()
        {
            return Task.FromResult("");
        }

        public Task<string> GetProjectExecutableNameAsync()
        {
            return Task.FromResult("");
        }

        public Task<List<string>> GetFrcDependenciesAsync()
        {
            return Task.FromResult(new List<string>());
        }

        public Task SetFrcDependenciesAsync(IList<(string dep, string version)> dependencies)
        {
            return Task.CompletedTask;
        }

        public Task SetFrcTooling((string tool, string version) tool)
        {
            return Task.CompletedTask;
        }
    }

    public class DotNetProjectInformationProvider : IProjectInformationProvider
    {
        public MsBuildProject BuildProject { get; }
        private string m_projectRoot;
        private string m_buildDirectory;
        private string m_executableName;
        public DotNetProjectInformationProvider(MsBuildProject msBuild)
        {
            BuildProject = msBuild;
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

        public Task<List<string>> GetFrcDependenciesAsync()
        {
            return Task.FromResult(BuildProject.GetWPILibPackages());
        }

        public Task SetFrcDependenciesAsync(IList<(string dep, string version)> dependencies)
        {
            BuildProject.SetWPILibPackages(dependencies);
            return Task.CompletedTask;
        }

        public Task SetFrcTooling((string tool, string version) tool)
        {
            BuildProject.SetDotNetToolingVersion(tool);
            return Task.CompletedTask;
        }
    }
}
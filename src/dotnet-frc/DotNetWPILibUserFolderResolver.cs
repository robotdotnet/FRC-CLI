using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class DotNetWPILibUserFolderResolver : IWPILibUserFolderResolver
    {
        private IProjectInformationProvider m_projectInformationProvider;

        public DotNetWPILibUserFolderResolver(IProjectInformationProvider projectInformationProvider)
        {
            m_projectInformationProvider = projectInformationProvider;
        }

        public async Task<string> GetWPILibUserFolderAsync()
        {
            // Netstandard does not support a way to get the user folder, so we will just use a WPILib folder in the project.
            string projectFolder = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            return Path.Combine(projectFolder, "wpilib");
        }
    }
}
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Base.Models;
using Newtonsoft.Json;

namespace FRC.CLI.Common.Implementations
{
    public class JsonFrcSettingsProvider : IFrcSettingsProvider
    {
        public const string SettingsJsonFileName = "frcsettings.json";

        IExceptionThrowerProvider m_exceptionThrowerProvider;
        IOutputWriter m_outputWriter;
        IProjectInformationProvider m_projectInformationProvider;

        public JsonFrcSettingsProvider(IExceptionThrowerProvider exceptionThrowerProvider,
            IOutputWriter outputWriter, IProjectInformationProvider projectInformationProvider)
        {
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_outputWriter = outputWriter;
            m_projectInformationProvider = projectInformationProvider;
        }

        public async Task<FrcSettings> GetFrcSettingsAsync()
        {
            string projectDirectory = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            string settingsFile = Path.Combine(projectDirectory, SettingsJsonFileName);
            if (!File.Exists(settingsFile))
            {
                return null;
            }
            string json = File.ReadAllText(settingsFile);
            var settingsStore = await Task.Run(() => JsonConvert.DeserializeObject<FrcSettings>(json)).ConfigureAwait(false);
            return settingsStore;
            
        }
    }
}
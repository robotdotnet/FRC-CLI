using System;
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

        FrcSettings m_settingsStore;

        public JsonFrcSettingsProvider(IExceptionThrowerProvider exceptionThrowerProvider,
            IOutputWriter outputWriter, IProjectInformationProvider projectInformationProvider)
        {
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_outputWriter = outputWriter;
            m_projectInformationProvider = projectInformationProvider;
            m_settingsStore = null;
        }

        public async Task<FrcSettings> GetFrcSettingsAsync()
        {
            if (m_settingsStore != null)
            {
                return m_settingsStore;
            }
            string projectDirectory = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            string settingsFile = Path.Combine(projectDirectory, SettingsJsonFileName);
            if (!File.Exists(settingsFile))
            {
                return null;
            }
            string json = File.ReadAllText(settingsFile);
            m_settingsStore = await Task.Run(() => JsonConvert.DeserializeObject<FrcSettings>(json)).ConfigureAwait(false);
            return m_settingsStore;
            
        }
    }
}
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
            string json = await Task.Run(() => File.ReadAllText(settingsFile)).ConfigureAwait(false);
            var settingsStore = await Task.Run(() => JsonConvert.DeserializeObject<FrcSettings>(json)).ConfigureAwait(false);
            return settingsStore;
            
        }

        public async Task<bool> WriteFrcSettingsAsync(FrcSettings settings)
        {
            string serialized = await Task.Run(() => JsonConvert.SerializeObject(settings, 
                Formatting.Indented)).ConfigureAwait(false);
            if (string.IsNullOrEmpty(serialized))
            {
                return false;
            }
            string projectDirectory = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            string settingsFile = Path.Combine(projectDirectory, SettingsJsonFileName);
            await Task.Run(() => File.WriteAllText(settingsFile, serialized)).ConfigureAwait(false);
            return true;
        }
    }
}
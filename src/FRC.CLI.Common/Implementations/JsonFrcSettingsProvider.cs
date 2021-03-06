using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Base.Models;
using Newtonsoft.Json;

namespace FRC.CLI.Common.Implementations
{
    public class JsonFrcSettingsProvider : IFrcSettingsProvider
    {
        public static readonly string SettingsJsonFileName = Path.Join(".wpilib", "wpilib_preferences.json");
        private readonly IExceptionThrowerProvider m_exceptionThrowerProvider;
        private readonly IOutputWriter m_outputWriter;
        private readonly IProjectInformationProvider m_projectInformationProvider;
        private readonly IBuildSettingsProvider m_buildSettingsProvider;
        private readonly IFileReaderProvider m_fileReaderProvider;

        public JsonFrcSettingsProvider(IExceptionThrowerProvider exceptionThrowerProvider,
            IOutputWriter outputWriter, IProjectInformationProvider projectInformationProvider,
            IBuildSettingsProvider buildSettingsProvider, IFileReaderProvider fileReaderProvider)
        {
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_outputWriter = outputWriter;
            m_projectInformationProvider = projectInformationProvider;
            m_buildSettingsProvider = buildSettingsProvider;
            m_fileReaderProvider = fileReaderProvider;
        }

        public async Task<FrcSettings?> GetFrcSettingsAsync()
        {
            bool verbose = m_buildSettingsProvider.Verbose;
            if (verbose)
            {
                await m_outputWriter.WriteLineAsync("Getting FRC Settings File").ConfigureAwait(false);
            }
            string projectDirectory = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            string settingsFile = Path.Combine(projectDirectory, SettingsJsonFileName);
            string? json = null;
            try
            {
                json = await m_fileReaderProvider.ReadFileAsStringAsync(settingsFile).ConfigureAwait(false);
            }
            catch (IOException)
            {
                // Locked or missing file, failed to read
                if (verbose)
                {
                    await m_outputWriter.WriteLineAsync("Could not read from settings file").ConfigureAwait(false);
                }
                return null;
            }
            var deserializeSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            try
            {
                var settingsStore = await Task.Run(() => JsonConvert.DeserializeObject<FrcSettings>(json,
                    deserializeSettings)).ConfigureAwait(false);
                if (verbose)
                {
                    await m_outputWriter.WriteLineAsync("Successfully read settings file").ConfigureAwait(false);
                }
                return settingsStore;
            }
            catch (JsonSerializationException)
            {
                if (verbose)
                {
                    await m_outputWriter.WriteLineAsync("Could not parse settings file").ConfigureAwait(false);
                }
                return null;
            }
        }

        public async Task WriteFrcSettingsAsync(FrcSettings settings)
        {
            await m_outputWriter.WriteLineAsync("Writing settings file").ConfigureAwait(false);
            string serialized = await Task.Run(() => JsonConvert.SerializeObject(settings,
                Formatting.Indented)).ConfigureAwait(false);
            if (string.IsNullOrEmpty(serialized))
            {
                m_exceptionThrowerProvider.ThrowException("Failed to serialize settings file");
            }
            string projectDirectory = await m_projectInformationProvider.GetProjectRootDirectoryAsync().ConfigureAwait(false);
            string settingsFile = Path.Combine(projectDirectory, SettingsJsonFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(settingsFile));
            await File.WriteAllTextAsync(settingsFile, serialized);
        }
    }
}
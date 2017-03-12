using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using Newtonsoft.Json;

namespace FRC.CLI.Common.Implementations
{
    public class WPILibImageSettingsProvider : IWPILibImageSettingsProvider
    {
        public const string JsonFileName = "ImageSettings.json";

        private IProjectInformationProvider m_projectInformationProvider;
        private INativeContentDeploymentProvider m_nativeContentDeploymentProvider;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;
        private IFileReaderProvider m_fileReaderProvider;

        public WPILibImageSettingsProvider(IProjectInformationProvider projectInformationProvider,
            INativeContentDeploymentProvider nativeContentDeploymentProvider,
            IExceptionThrowerProvider exceptionThrowerProvider,
            IFileReaderProvider fileReaderProvider)
        {
            m_projectInformationProvider = projectInformationProvider;
            m_nativeContentDeploymentProvider = nativeContentDeploymentProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_fileReaderProvider = fileReaderProvider;
        }

        public IEnumerable<string> ParseStringToJson(string json)
        {
            var definition = new {
                AllowedImages = new string[0]
            };
            var settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Ignore
            };
            var settingsStore = JsonConvert.DeserializeAnonymousType(json, definition, settings);
            return settingsStore.AllowedImages;
        }

        public virtual string GetCombinedFilePath(string buildLocation)
        {
            string nativeFolder = Path.Combine(buildLocation, m_nativeContentDeploymentProvider.NativeDirectory);
            string settingsFile = Path.Combine(nativeFolder, JsonFileName);
            return settingsFile;
        }

        public async Task<IEnumerable<string>> GetAllowedImageVersionsAsync()
        {
            string buildLocation = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);

            string settingsFile = GetCombinedFilePath(buildLocation);

            string json = null;

            try
            {
                json = await m_fileReaderProvider.ReadFileAsStringAsync(settingsFile).ConfigureAwait(false);
            }
            catch (IOException)
            {
                throw m_exceptionThrowerProvider.ThrowException($"Failed to read settings file at {settingsFile}");
            }

            try
            {
                return ParseStringToJson(json);
            }
            catch (JsonSerializationException)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to parse image settings file");
            }
            catch (JsonReaderException)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to parse image settings file");
            }
        }
    }
}
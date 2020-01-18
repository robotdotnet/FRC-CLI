using System.Collections.Generic;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Linq;
using System.Net;
using FRC.CLI.Base.Enums;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioImageProvider : IRoboRioImageProvider
    {
        private readonly IWPILibImageSettingsProvider m_wpilibImageSettingsProvider;
        private readonly IExceptionThrowerProvider m_exceptionThrowerProvider;
        private readonly IFileDeployerProvider m_fileDeployerProvider;
        public RoboRioImageProvider(IWPILibImageSettingsProvider wpilibImageSettingsProvider,
            IExceptionThrowerProvider exceptionThrowerProvider,
            IFileDeployerProvider fileDeployerProvider)
        {
            m_wpilibImageSettingsProvider = wpilibImageSettingsProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_fileDeployerProvider = fileDeployerProvider;
        }

        public async Task CheckCorrectImageAsync()
        {
            string currentImage = await GetCurrentRoboRioImageAsync().ConfigureAwait(false);
            IEnumerable<string> allowedImages = await GetAllowedRoboRioImagesAsync().ConfigureAwait(false);
            if (!allowedImages.Contains(currentImage))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("RoboRIO image not correct. Allowed Images:");
                foreach (var item in allowedImages)
                {
                    builder.AppendLine($"    {item}");
                }
                builder.AppendLine($"Current Version: {currentImage}");
                throw m_exceptionThrowerProvider.ThrowException(builder.ToString());
            }
        }

        public Task<IEnumerable<string>> GetAllowedRoboRioImagesAsync()
        {
            return m_wpilibImageSettingsProvider.GetAllowedImageVersionsAsync();
        }

        public async Task<string> GetCurrentRoboRioImageAsync()
        {
            var result = await m_fileDeployerProvider.RunCommandAsync("grep IMAGEVERSION /etc/natinst/share/scs_imagemetadata.ini", ConnectionUser.LvUser).ConfigureAwait(false);
            var image = result.Result;
            var start = image.IndexOf("FRC_roboRIO");
            if (start < 0) return "";
            image = image.Substring(start);
            image = image.Substring(0, image.IndexOf('\"'));
            return image;
        }
    }
}
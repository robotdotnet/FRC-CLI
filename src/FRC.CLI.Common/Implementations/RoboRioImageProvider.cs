using System.Collections.Generic;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Linq;
using System.Net;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioImageProvider : IRoboRioImageProvider
    {
        private IWPILibImageSettingsProvider m_wpilibImageSettingsProvider;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;
        private IFileDeployerProvider m_fileDeployerProvider;
        public RoboRioImageProvider(IWPILibImageSettingsProvider wpilibImageSettingsProvider, 
            IExceptionThrowerProvider exceptionThrowerProvider,
            IFileDeployerProvider fileDeployerProvider)
        {
            m_wpilibImageSettingsProvider = wpilibImageSettingsProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_fileDeployerProvider = fileDeployerProvider;
        }

        public async Task<bool> CheckCorrectImageAsync()
        {
            string currentImage = await GetCurrentRoboRioImageAsync().ConfigureAwait(false);
            IList<string> allowedImages = await GetAllowedRoboRioImagesAsync().ConfigureAwait(false);
            return allowedImages.Contains(currentImage);
        }

        public async Task<IList<string>> GetAllowedRoboRioImagesAsync()
        {
            return await m_wpilibImageSettingsProvider.GetAllowedImageVersionsAsync().ConfigureAwait(false);
        }

        public async Task<string> GetCurrentRoboRioImageAsync()
        {
            using (HttpClient wc = new HttpClient())
            { 
                var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Function", "GetPropertiesOfItem"),
                    new KeyValuePair<string, string>("Plugins", "nisyscfg"),
                    new KeyValuePair<string, string>("Items", "system")
                });

                IPAddress connectionIp = await m_fileDeployerProvider.GetConnectionIpAsync().ConfigureAwait(false);
                var reqResult = await wc.PostAsync($"http://{connectionIp.ToString()}/nisysapi/server", content).ConfigureAwait(false);
                var result = await reqResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                var sstring = Encoding.Unicode.GetString(result);

                var doc = new XmlDocument();
                doc.LoadXml(sstring);

                var vals = doc.GetElementsByTagName("Property");

                string str = null;

                foreach (XmlElement val in vals.Cast<XmlElement>().Where(val => val.InnerText.Contains("FRC_roboRIO")))
                {
                    str = val.InnerText;
                }

                return str;
            }
        }
    }
}
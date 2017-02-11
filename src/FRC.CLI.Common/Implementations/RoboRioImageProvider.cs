using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Linq;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioImageProvider : IRoboRioImageProvider
    {
        private IWPILibDeploySettingsProvider m_wpilibDeploySettingsProvider;
        public RoboRioImageProvider(IWPILibDeploySettingsProvider wpilibDeploySettingsProvider)
        {
            m_wpilibDeploySettingsProvider = wpilibDeploySettingsProvider;
        }

        public async Task<bool> CheckCorrectImageAsync(IFileDeployerProvider fileDeployerProvider)
        {
            string currentImage = await GetCurrentRoboRioImageAsync(fileDeployerProvider).ConfigureAwait(false);
            IList<string> allowedImages = await GetAllowedRoboRioImagesAsync().ConfigureAwait(false);
            return allowedImages.Contains(currentImage);
        }

        public async Task<IList<string>> GetAllowedRoboRioImagesAsync()
        {
            return await m_wpilibDeploySettingsProvider.GetAllowedImageVersionsAsync().ConfigureAwait(false);
        }

        public async Task<string> GetCurrentRoboRioImageAsync(IFileDeployerProvider fileDeployerProvider)
        {
            using (HttpClient wc = new HttpClient())
            { 
                var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Function", "GetPropertiesOfItem"),
                    new KeyValuePair<string, string>("Plugins", "nisyscfg"),
                    new KeyValuePair<string, string>("Items", "system")
                });

                var reqResult = await wc.PostAsync($"http://{fileDeployerProvider.ConnectionIp.ToString()}/nisysapi/server", content).ConfigureAwait(false);
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
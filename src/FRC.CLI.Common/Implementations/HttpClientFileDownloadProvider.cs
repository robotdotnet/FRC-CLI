using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class HttpClientFileDownloadProvider : IFileDownloadProvider
    {
        IOutputWriter m_outputWriter;

        public HttpClientFileDownloadProvider(IOutputWriter outputWriter)
        {
            m_outputWriter = outputWriter;
        }

        public async Task DownloadFileAsync(string url, string outputLocation, string outputFileName)
        {
            await m_outputWriter.WriteLineAsync($"Downloading file: {url}");
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    using (var readStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        Directory.CreateDirectory(outputLocation);
                        string file = Path.Combine(outputLocation, outputFileName);
                        await m_outputWriter.WriteLineAsync($"Writing file to: {file}");
                        using (var writeStream = File.Open(file, FileMode.Create))
                        {
                            await readStream.CopyToAsync(writeStream).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}
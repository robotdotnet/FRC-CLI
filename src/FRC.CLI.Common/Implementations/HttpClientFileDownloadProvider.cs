using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class HttpClientFileDownloadProvider : IFileDownloadProvider
    {
        IOutputWriter m_outputWriter;
        IExceptionThrowerProvider m_exceptionThrowerProvider;

        public HttpClientFileDownloadProvider(IOutputWriter outputWriter,
            IExceptionThrowerProvider exceptionThrowerProvider)
        {
            m_outputWriter = outputWriter;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
        }

        public async Task DownloadFileToFileAsync(string url, string outputLocation, string outputFileName)
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

        public async Task DownloadFileToStreamAsync(string url, Stream outputStream)
        {
            await m_outputWriter.WriteLineAsync($"Downloading file to stream: {url}");
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    using (var readStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        await readStream.CopyToAsync(outputStream).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
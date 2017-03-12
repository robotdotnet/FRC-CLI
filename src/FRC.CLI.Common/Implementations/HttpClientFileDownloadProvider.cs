using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class HttpClientFileDownloadProvider : IFileDownloadProvider
    {
        private IOutputWriter m_outputWriter;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;

        public HttpMessageHandler MessageHandler { get; set; }

        public HttpClientFileDownloadProvider(IOutputWriter outputWriter,
            IExceptionThrowerProvider exceptionThrowerProvider)
        {
            m_outputWriter = outputWriter;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
        }

        public async Task DownloadFileToStreamAsync(string url, Stream outputStream)
        {
            await m_outputWriter.WriteLineAsync($"Downloading file to stream: {url}").ConfigureAwait(false);
            using (HttpClient client = 
                MessageHandler == null ? new HttpClient()
                                       : new HttpClient(MessageHandler))
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
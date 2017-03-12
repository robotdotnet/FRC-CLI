using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class HttpPostRequestProvider : IPostRequestProvider
    {
        public async Task<string> GetPostRequestAsync(string url, FormUrlEncodedContent content)
        {
            using (var wc = new HttpClient())
            {
                var reqResult = await wc.PostAsync(url, content).ConfigureAwait(false);
                var result = await reqResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                return Encoding.Unicode.GetString(result);
            }
        }
    }
}
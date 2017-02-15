using System.Net.Http;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IPostRequestProvider
    {
         Task<string> GetPostRequestAsync(string url, FormUrlEncodedContent content);
    }
}
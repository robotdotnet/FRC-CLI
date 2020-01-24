using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class Md5HashCheckerProvider : IMd5HashCheckerProvider
    {
        public async Task<bool> VerifyMd5Hash(string file, string hash)
        {
            string? sum = await MD5Helper.Md5SumAsync(file).ConfigureAwait(false);
            return sum != null && sum.Equals(hash, System.StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
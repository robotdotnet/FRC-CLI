using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface IMd5HashCheckerProvider
    {
         Task<bool> VerifyMd5Hash(string file, string hash);
    }
}
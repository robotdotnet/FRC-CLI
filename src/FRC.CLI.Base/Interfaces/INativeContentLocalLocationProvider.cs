using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface INativeContentLocalLocationProvider
    {
         Task<string> GetNativeContentLocationAsync();
    }
}
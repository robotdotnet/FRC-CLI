using System;
using System.Threading.Tasks;

namespace FRC.CLI.Base.Interfaces
{
    public interface ICodeBuilderProvider
    {
        /// <summary>
        /// Builds the code needed to be deployed
        /// </summary>
        /// <param name="debug">True to build in debug mode</param>
        /// <param name="projectFileLoc">The location of the project file to build</param>
        /// <returns>Tuple containing the build exit status, and the output location</returns>
        Task<Tuple<int, string, string>> BuildCodeAsync();
    }
}
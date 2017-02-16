using System;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class FileReaderProvider : IFileReaderProvider
    {
        public async Task<string> ReadFileAsStringAsync(string file)
        {
            return await Task.Run(() => File.ReadAllText(file)).ConfigureAwait(false);
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class FileReaderProvider : IFileReaderProvider
    {
        public Task<string> ReadFileAsStringAsync(string file)
        {
            return File.ReadAllTextAsync(file);
        }
    }
}
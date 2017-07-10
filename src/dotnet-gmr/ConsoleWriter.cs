using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class ConsoleWriter : IOutputWriter
    {
        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        public Task WriteLineAsync(string line)
        {
            Console.WriteLine(line);
            return Task.CompletedTask;
        }
    }
}
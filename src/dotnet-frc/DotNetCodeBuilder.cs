using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using Microsoft.DotNet.Cli.Utils;

namespace dotnet_frc
{
    public class DotNetCodeBuilder : ICodeBuilder
    {
        public async Task<Tuple<int, string>> BuildCodeAsync(bool debug, string projectFileLoc)
        {
            return await Task.Run(() =>
            {
                string outputLoc = "bin\\frctemp";
                var cmdArgs = new List<string>
                {
                    projectFileLoc,
                    "--configuration", "Release",
                    "-o", outputLoc
                };

                var result = Command.CreateDotNet("build", cmdArgs).Execute();
                return new Tuple<int, string>(result.ExitCode, outputLoc);
            });
        }
    }
}
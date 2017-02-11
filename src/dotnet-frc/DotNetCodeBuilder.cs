using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using Microsoft.DotNet.Cli.Utils;

namespace dotnet_frc
{
    public class DotNetCodeBuilder : ICodeBuilderProvider
    {
        private MsBuildProject m_projectFile;

        public DotNetCodeBuilder(MsBuildProject projectFile)
        {
            m_projectFile = projectFile;
        }

        public async Task<Tuple<int, string, string>> BuildCodeAsync()
        {
            return await Task.Run(() =>
            {
                string outputLoc = "bin\\frctemp";
                var cmdArgs = new List<string>
                {
                    m_projectFile.ProjectDirectory,
                    "--configuration", "Release",
                    "-o", outputLoc
                };

                var result = Command.CreateDotNet("build", cmdArgs).Execute();
                return new Tuple<int, string, string>(result.ExitCode, Path.Combine(m_projectFile.ProjectDirectory, outputLoc), m_projectFile.GetProjectAssemblyName());
            });
        }
    }
}
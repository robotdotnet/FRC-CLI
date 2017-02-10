using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;

namespace dotnet_frc
{
    class Program
    {
        static void Main(string[] args)
        {
            Project project;
            var currentDirectory = @"C:\Users\thadh\Documents\GitHub\RobotDotNet\NetworkTables\src\FRC.NetworkTables";
            if (ProjectReader.TryGetProject(currentDirectory, out project))
            {
                if (project.Files.SourceFiles.Any())
                {
                    Console.WriteLine("Files:");
                    foreach (var file in project.Files.SourceFiles)
                        Console.WriteLine("  {0}", file.Replace(currentDirectory, ""));
                }
                if (project.Dependencies.Any())
                {
                    Console.WriteLine("Dependencies:");
                    foreach (var dependancy in project.Dependencies)
                    {
                        Console.WriteLine("  {0} - Line:{1}, Column:{2}",
                                dependancy.SourceFilePath.Replace(currentDirectory, ""),
                                dependancy.SourceLine,
                                dependancy.SourceColumn);
                    }
                }
            }

            // Create a workspace
            var workspace = new BuildWorkspace(ProjectReaderSettings.ReadFromEnvironment());

            // Fetch the ProjectContexts
            var projectPath = project.ProjectFilePath;
            /*
            var runtimeIdentifiers = 
                RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers();
            var projectContexts = workspace.GetProjectContextCollection(projectPath)
                //.EnsureValid(projectPath)
                .FrameworkOnlyContexts
                .Select(c => workspace.GetRuntimeContext(c, runtimeIdentifiers))
                .ToList();

            // Setup the build arguments
            var projectContextToBuild = projectContexts.First();
            */
            var cmdArgs = new List<string>
            {
                projectPath,
                "--configuration", "Release",
            };

            // Build!!
            //Console.WriteLine("Building Project for {0}", projectContextToBuild.RuntimeIdentifier);
            var result = Command.CreateDotNet("build", cmdArgs).Execute();
            Console.WriteLine("Build {0}", result.ExitCode == 0 ? "SUCCEEDED" : "FAILED");


            Console.WriteLine("Hello World!");
        }
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools;
using Microsoft.DotNet.Tools.Common;

namespace dotnet_frc
{
    public class MsBuildProject
    {
        public string ProjectDirectory { get; }
        public string ProjectFile { get; }

        public ProjectRootElement ProjectRoot { get; }

        private ProjectCollection _projects;

        private MsBuildProject(ProjectCollection projects, ProjectRootElement project)
        {
            _projects = projects;
            ProjectRoot = project;
            ProjectFile = project.FullPath;



            ProjectDirectory = PathUtility.EnsureTrailingSlash(project.DirectoryPath);
        }

        public bool GetIsWPILibProject()
        {
            foreach(var item in ProjectRoot.Items)
            {
                if (item.ItemType == "PackageReference")
                {
                    if (item.Include == "FRC.WPILib")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<string> GetWPILibPackages()
        {
            List<string> packages = new List<string>();
            foreach(var item in ProjectRoot.Items)
            {
                if (item.ItemType == "PackageReference")
                {
                    if (item.Include.Contains("FRC."))
                    {
                        packages.Add(item.Include);
                    }
                }
            }
            return packages;
        }

        public void SetWPILibPackages(IList<(string dep, string version)> dependencies)
        {
            foreach(var toSet in dependencies)
            {
                foreach(var item in ProjectRoot.Items)
                {
                    if (item.ItemType == "PackageReference")
                    {
                        if (item.Include == toSet.dep)
                        {
                            foreach(var child in item.Children)
                            {
                                if (child is ProjectMetadataElement childData)
                                {
                                    if (childData.Name == "Version")
                                    {
                                        childData.Value = toSet.version;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            ProjectRoot.Save();
        }

        public void SetDotNetToolingVersion((string tool, string version) tooling)
        {
            foreach (var item in ProjectRoot.Items)
            {
                if (item.ItemType == "DotNetCliToolReference")
                {
                    if (item.Include == tooling.tool)
                    {
                        foreach(var child in item.Children)
                        {
                            if (child is ProjectMetadataElement childData)
                            {
                                if (childData.Name == "Version")
                                {
                                    childData.Value = tooling.version;
                                }
                            }
                        }
                    }
                }
            }
            ProjectRoot.Save();
        }

        public string GetProjectAssemblyName()
        {
            foreach(var item in ProjectRoot.Properties)
            {
                if (item.Name == "AssemblyName")
                {
                    return item.Value + ".exe";
                }
            }
            return Path.GetFileNameWithoutExtension(ProjectFile) + ".exe";
        }

        public MsBuildProject GetPropsFile()
        {
            var projectFileName = Directory.GetFiles(Path.Combine(ProjectDirectory, "obj")).Where(x => x.EndsWith("csproj.nuget.g.props")).FirstOrDefault();

            var project = TryOpenProject(_projects, projectFileName);
            if (project == null)
            {
                return null;
            }

            return new MsBuildProject(_projects, project);
        }

        public static MsBuildProject FromFileOrDirectory(ProjectCollection projects, string fileOrDirectory)
        {
            if (File.Exists(fileOrDirectory))
            {
                return FromFile(projects, fileOrDirectory);
            }
            else
            {
                return FromDirectory(projects, fileOrDirectory);
            }
        }

        public static MsBuildProject FromFile(ProjectCollection projects, string projectPath)
        {
            if (!File.Exists(projectPath))
            {
                throw new GracefulException(CommonLocalizableStrings.ProjectDoesNotExist, projectPath);
            }

            var project = TryOpenProject(projects, projectPath);
            if (project == null)
            {
                throw new GracefulException(CommonLocalizableStrings.ProjectIsInvalid, projectPath);
            }

            return new MsBuildProject(projects, project);
        }

        public static MsBuildProject FromDirectory(ProjectCollection projects, string projectDirectory)
        {
            FileInfo projectFile = GetProjectFileFromDirectory(projectDirectory);

            var project = TryOpenProject(projects, projectFile.FullName);
            if (project == null)
            {
                throw new GracefulException(CommonLocalizableStrings.FoundInvalidProject, projectFile.FullName);
            }

            return new MsBuildProject(projects, project);
        }

        public static FileInfo GetProjectFileFromDirectory(string projectDirectory)
        {
            DirectoryInfo dir;
            try
            {
                dir = new DirectoryInfo(projectDirectory);
            }
            catch (ArgumentException)
            {
                throw new GracefulException(CommonLocalizableStrings.CouldNotFindProjectOrDirectory, projectDirectory);
            }

            if (!dir.Exists)
            {
                throw new GracefulException(CommonLocalizableStrings.CouldNotFindProjectOrDirectory, projectDirectory);
            }

            FileInfo[] files = dir.GetFiles("*proj");
            if (files.Length == 0)
            {
                throw new GracefulException(
                    CommonLocalizableStrings.CouldNotFindAnyProjectInDirectory,
                    projectDirectory);
            }

            if (files.Length > 1)
            {
                throw new GracefulException(CommonLocalizableStrings.MoreThanOneProjectInDirectory, projectDirectory);
            }

            return files.First();
        }

        private static ProjectRootElement TryOpenProject(ProjectCollection projects, string filename)
        {
            try
            {
                return ProjectRootElement.Open(filename, projects, preserveFormatting: true);
            }
            catch (InvalidProjectFileException)
            {
                return null;
            }
        }
    }
}
using Autofac;
using Microsoft.Build.Evaluation;
using Microsoft.DotNet.Tools.Common;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace dotnet_frc.Commands
{
    public abstract class SubCommandBase : Command
    {

        public Option<int> TeamOption { get; }
        public Option<bool> VerboseOption { get; }
        protected Option<string> ProjectOption { get; }

        protected SubCommandBase(string name, string description) : base(name, description)
        {
            TeamOption = new Option<int>("--team");
            TeamOption.Description = "Force a team number";
            TeamOption.Argument.SetDefaultValue(-1);
            TeamOption.AddAlias("-t");
            TeamOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            Add(TeamOption);

            ProjectOption = new Option<string>("--project");
            ProjectOption.Description = "Select a project if not the current folder";
            ProjectOption.AddAlias("-p");
            ProjectOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            Add(ProjectOption);
        }

        protected MsBuildProject? ResolveProject(string? project)
        {

            //builder.Register(c =>
            //{
            //    MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, fileOrDirectory);
            //    return new DotNetProjectInformationProvider(msBuild);
            //}).As<IProjectInformationProvider>();

            if (project == null)
            {

                project = Directory.GetCurrentDirectory();
                project = PathUtility.EnsureTrailingSlash(project);
            }

            try
            {
                return MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, project);
            }
            catch
            {
                return null;
            }
        }



        protected void EnsureEmptyIfNull<T>([NotNull]ref T[]? arr)
        {
            if (arr == null)
            {
                arr = Array.Empty<T>();
            }
        }
    }
}

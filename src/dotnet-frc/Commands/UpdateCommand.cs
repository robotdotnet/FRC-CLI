using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common.Implementations;
using Microsoft.Build.Evaluation;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;

namespace dotnet_frc
{
    internal class UpdateCommand : FrcSubCommandBase
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private CommandOption _toolsOption;
        private CommandOption _dependenciesOption;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public static DotNetSubCommandBase Create()
        {
            var command = new UpdateCommand
            {
                Name = "update",
                Description = "Updates tools and WPILib dependencies in the project",
                HandleRemainingArguments = false
            };

            SetupBaseOptions(command, false);

            command._toolsOption = command.Option(
                "-t|--tools",
                "Update the tooling",
                CommandOptionType.NoValue
            );

            command._dependenciesOption = command.Option(
                "-d|--dependencies",
                "Update the dependencies",
                CommandOptionType.NoValue
            );

            return command;
        }

        public override async Task<int> RunAsync(string fileOrDirectory)
        {
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
                false, false);
            builder.Register(c =>
            {
                MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, fileOrDirectory);
                return new DotNetProjectInformationProvider(msBuild);
            }).As<DotNetProjectInformationProvider>();

            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                if (!_toolsOption.HasValue() && !_dependenciesOption.HasValue())
                {
                    throw scope.Resolve<IExceptionThrowerProvider>().ThrowException(
                        "No argument specified. Must provide an argument to use"
                    );
                }

                var project = scope.Resolve<DotNetProjectInformationProvider>();

                if (_dependenciesOption.HasValue())
                {
                    List<(string, string)> packageVersions = new List<(string, string)>();
                    var packages = await project.GetFrcDependenciesAsync();
                    foreach(var package in packages)
                    {
                        var newest = await DotNetNugetVersionDetector.FindNewestPackageVersion(package, true);
                        if (newest == null) continue;
                        packageVersions.Add((package, newest));
                    }
                    await project.SetFrcDependenciesAsync(packageVersions);
                }

                if (_toolsOption.HasValue())
                {
                    string toolName = "FRC.DotNet.CLI";
                    var newest = await DotNetNugetVersionDetector.FindNewestPackageVersion(toolName, true);
                    if (newest != null)
                    {
                        await project.SetFrcTooling((toolName, newest));
                    }
                }

                var writer = scope.Resolve<IOutputWriter>();
                await writer.WriteLineAsync("Dependencies have been updated. Please run dotnet restore.");
            }
            return 0;
        }
    }
}
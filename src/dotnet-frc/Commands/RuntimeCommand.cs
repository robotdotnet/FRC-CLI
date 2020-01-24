using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common.Implementations;

namespace dotnet_frc.Commands
{
    internal class RuntimeCommand : SubCommandBase
    {
        private Option downloadOption;
        private Option installOption;
        private Option checkOption;
        private Option<string> locationOption;



        public RuntimeCommand() : base("runtime", "Installs the runtime on the robot")
        {
            TreatUnmatchedTokensAsErrors = true;
            downloadOption = new Option("--download");
            downloadOption.AddAlias("-d");
            downloadOption.Description = "Download mono from internet";
            downloadOption.Argument.Arity = ArgumentArity.Zero;
            Add(downloadOption);

            installOption = new Option("--install");
            installOption.AddAlias("-i");
            installOption.Description = "Install mono onto rio";
            installOption.Argument.Arity = ArgumentArity.Zero;
            Add(installOption);

            locationOption = new Option<string>("--location");
            locationOption.AddAlias("-l");
            locationOption.Description = "Point to a predownloaded mono location";
            //locationOption.ARgArity = ArgumentArity.ZeroOrOne;
            Add(locationOption);

            this.AddValidator(ValidateOneOptionSelected);

            Handler = CommandHandler.Create<int, string?, bool, bool, string?>(HandleCommand);
        }

        public string? ValidateOneOptionSelected(CommandResult result)
        {
            var isInstall = result.Children["install"] != null;
            var isDownload = result.Children["download"] != null;
            var isCheck = result.Children["check"] != null;
            if (!isInstall && !isDownload && !isCheck)
            {
                return "Either install, download or check must be selected";
            }
            return null;
        }

        public async Task<int> HandleCommand(int team, string? project, bool download, bool install, string? location)
        {
            var msBuild = ResolveProject(project);
            if (msBuild == null)
            {

                return -1;
            }
            var builder = new ContainerBuilder();
            AutoFacUtilites.AddCommonServicesToContainer(builder, msBuild, team, false,
                false);
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var runtimeProvider = scope.Resolve<IRuntimeProvider>();
                if (download)
                {
                    await runtimeProvider.DownladRuntimeAsync();
                }

                if (install)
                {
                    if (location != null)
                    {
                        await runtimeProvider.InstallRuntimeAsync(location);
                    }
                    else
                    {
                        await runtimeProvider.InstallRuntimeAsync();
                    }
                }
            }

            return 0;
        }

        //        private CommandOption _downloadOption;
        //        private CommandOption _installOption;
        //        private CommandOption _locationOption;
        //#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        //        public static DotNetSubCommandBase Create()
        //        {
        //            var command = new RuntimeCommand
        //            {
        //                Name = "runtime",
        //                Description = "Installs the runtime on the robot",
        //                HandleRemainingArguments = false
        //            };

        //            SetupBaseOptions(command);

        //            command._downloadOption = command.Option(
        //                "-d|--download",
        //                "Download the runtime",
        //                CommandOptionType.NoValue
        //            );

        //            command._installOption = command.Option(
        //                "-i|--install",
        //                "Install the runtime",
        //                CommandOptionType.NoValue
        //            );

        //            command._locationOption = command.Option(
        //                "-l|--location",
        //                "Local runtime location",
        //                CommandOptionType.SingleValue
        //            );

        //            return command;
        //        }

        //public override async Task<int> RunAsync(string fileOrDirectory)
        //{
        //    var builder = new ContainerBuilder();
        //    AutoFacUtilites.AddCommonServicesToContainer(builder, fileOrDirectory, this,
        //        false);
        //    builder.RegisterType<MonoRuntimeProvider>().As<IRuntimeProvider>();
        //    var container = builder.Build();

        //    using (var scope = container.BeginLifetimeScope())
        //    {
        //        if (!_downloadOption.HasValue() && !_installOption.HasValue())
        //        {
        //            throw scope.Resolve<IExceptionThrowerProvider>().ThrowException(
        //                "No argument specified. Must provide an argument to use"
        //            );
        //        }

        //        var runtimeProvider = scope.Resolve<IRuntimeProvider>();
        //        if (_downloadOption.HasValue())
        //        {
        //            await runtimeProvider.DownladRuntimeAsync();
        //        }

        //        if (_installOption.HasValue())
        //        {
        //            if (_locationOption.HasValue())
        //            {
        //                await runtimeProvider.InstallRuntimeAsync(_locationOption.Value());
        //            }
        //            else
        //            {
        //                await runtimeProvider.InstallRuntimeAsync();
        //            }
        //        }
        //    }

        //    return 0;
        //}

    }
}
using Autofac;
using dotnet_frc.Commands;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common;
using FRC.CLI.Common.Connections;
using FRC.CLI.Common.Implementations;
using Microsoft.Build.Evaluation;

namespace dotnet_frc
{
    internal static class AutoFacUtilites
    {
        public static void AddCommonServicesToContainer(ContainerBuilder builder, MsBuildProject msBuildProject,
            int teamNumber, bool verbose, bool debug, bool useTeam = true)
        {
            builder.RegisterType<ConsoleWriter>().As<IOutputWriter>();
            builder.RegisterType<JsonFrcSettingsProvider>().As<IFrcSettingsProvider>();
            builder.RegisterType<DotNetExceptionThrowerProvider>().As<IExceptionThrowerProvider>();
            builder.RegisterType<CodeDeployer>();
            builder.RegisterType<RobotCodeDeploymentProvider>().As<IRobotCodeDeploymentProvider>();
            builder.RegisterType<RoboRioDependencyCheckerProvider>().As<IRoboRioDependencyCheckerProvider>();
            builder.RegisterType<RoboRioImageProvider>().As<IRoboRioImageProvider>();
            builder.RegisterType<WPILibImageSettingsProvider>().As<IWPILibImageSettingsProvider>();
            builder.RegisterType<NativeContentDeploymentProvider>().As<INativeContentDeploymentProvider>();
            builder.RegisterType<WPILibNativeDeploySettingsProvider>().As<IWPILibNativeDeploySettingsProvider>();
            builder.RegisterType<DotNetCodeBuilder>().As<ICodeBuilderProvider>();
            builder.RegisterType<RoboRioConnection>().As<IFileDeployerProvider>().InstancePerLifetimeScope();
            builder.RegisterInstance(msBuildProject).AsSelf();
            builder.RegisterType<DotNetProjectInformationProvider>().As<IProjectInformationProvider>().InstancePerLifetimeScope();
            if (useTeam)
            {
                builder.RegisterType<DotNetTeamNumberProvider>().As<ITeamNumberProvider>().WithParameter(new TypedParameter(typeof(int?), 
                    DotNetTeamNumberProvider.GetTeamNumberFromCommandOption(teamNumber)));
            }
            builder.Register(c => new DotNetBuildSettingsProvider(debug, verbose)).As<IBuildSettingsProvider>();    
            builder.RegisterType<DotNetWPILibUserFolderResolver>().As<IWPILibUserFolderResolver>();
            builder.RegisterType<HttpClientFileDownloadProvider>().As<IFileDownloadProvider>();
            builder.RegisterType<RemotePackageInstallerProvider>().As<IRemotePackageInstallerProvider>();  
            builder.RegisterType<Md5HashCheckerProvider>().As<IMd5HashCheckerProvider>();
            builder.RegisterType<FileReaderProvider>().As<IFileReaderProvider>();
            builder.RegisterType<MonoRuntimeProvider>().As<IRuntimeProvider>();
        }
    }
}
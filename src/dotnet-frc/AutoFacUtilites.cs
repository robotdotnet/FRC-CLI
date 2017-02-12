using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common;
using FRC.CLI.Common.Connections;
using FRC.CLI.Common.Implementations;
using Microsoft.Build.Evaluation;

namespace dotnet_frc
{
    internal static class AutoFacUtilites
    {
        public static void AddCommonServicesToContainer(ContainerBuilder builder, string fileOrDirectory,
            FrcSubCommandBase command)
        {
            builder.RegisterType<ConsoleWriter>().As<IOutputWriter>();
            builder.RegisterType<DotNetExceptionThrowerProvider>().As<IExceptionThrowerProvider>();
            builder.RegisterType<JsonFrcSettingsProvider>().As<IFrcSettingsProvider>();
            builder.RegisterType<CodeDeployer>();
            builder.RegisterType<RobotCodeDeploymentProvider>().As<IRobotCodeDeploymentProvider>();
            builder.RegisterType<RoboRioDependencyCheckerProvider>().As<IRoboRioDependencyCheckerProvider>();
            builder.RegisterType<RoboRioImageProvider>().As<IRoboRioImageProvider>();
            builder.RegisterType<WPILibImageSettingsProvider>().As<IWPILibImageSettingsProvider>();
            builder.RegisterType<NativePackageDeploymentProvider>().As<INativePackageDeploymentProvider>();
            builder.RegisterType<WPILibNativeDeploySettingsProvider>().As<IWPILibNativeDeploySettingsProvider>();
            builder.RegisterType<DotNetCodeBuilder>().As<ICodeBuilderProvider>();
            builder.RegisterType<RoboRioConnection>().As<IFileDeployerProvider>().InstancePerLifetimeScope();
            builder.Register(c =>
            {
                MsBuildProject msBuild = MsBuildProject.FromFileOrDirectory(ProjectCollection.GlobalProjectCollection, fileOrDirectory);
                return new DotNetProjectInformationProvider(msBuild);
            }).As<IProjectInformationProvider>();
            builder.RegisterType<DotNetTeamNumberProvider>().As<ITeamNumberProvider>().WithParameter(new TypedParameter(typeof(int?), 
                DotNetTeamNumberProvider.GetTeamNumberFromCommandOption(command._teamOption)));
        }
    }
}
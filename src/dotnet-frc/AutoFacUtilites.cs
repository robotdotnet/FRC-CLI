using Autofac;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common;
using FRC.CLI.Common.Connections;
using FRC.CLI.Common.Implementations;

namespace dotnet_frc
{
    public static class AutoFacUtilites
    {
        public static void AddCommonServicesToContainer(ContainerBuilder builder)
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
        }
    }
}
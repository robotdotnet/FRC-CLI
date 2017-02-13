using System;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class MonoFileConstantsProvider : IMonoFileConstantsProvider
    {
        public string Url => DeployProperties.MonoUrl + DeployProperties.MonoVersion;

        public string Md5Sum => DeployProperties.MonoMd5;

        public string OutputFileName => DeployProperties.MonoVersion;
    }
}
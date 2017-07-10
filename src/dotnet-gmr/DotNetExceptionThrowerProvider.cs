using System;
using FRC.CLI.Base.Interfaces;
using Microsoft.DotNet.Cli.Utils;

namespace dotnet_frc
{
    public class DotNetExceptionThrowerProvider : IExceptionThrowerProvider
    {
        public Exception ThrowException(string msg)
        {
            throw new GracefulException(msg);
        }
    }
}
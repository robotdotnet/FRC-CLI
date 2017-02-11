using System;

namespace FRC.CLI.Base.Interfaces
{
    public interface IExceptionThrowerProvider
    {
         Exception ThrowException(string msg);
    }
}
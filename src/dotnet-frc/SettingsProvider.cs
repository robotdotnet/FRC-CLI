using System;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class SettingsProvider : ISettingsProvider
    {
        public bool Verbose => true;

        public bool DebugMode => false;
    }
}
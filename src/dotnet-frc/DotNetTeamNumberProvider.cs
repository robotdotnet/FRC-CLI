using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;
using Microsoft.DotNet.Cli.CommandLine;

namespace dotnet_frc
{
    public class DotNetTeamNumberProvider : ITeamNumberProvider
    {
        CommandOption m_teamOption;
        IFrcSettingsProvider m_frcSettingsProvider;

        public DotNetTeamNumberProvider(IFrcSettingsProvider frcSettingsProvider)
        {
            m_teamOption = teamOption;
        }

        public Task<int> GetTeamNumberAsync()
        {
            
        }
    }
}
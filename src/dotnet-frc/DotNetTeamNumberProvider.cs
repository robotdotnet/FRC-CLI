using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace dotnet_frc
{
    public class DotNetTeamNumberProvider : ITeamNumberProvider
    {
        private readonly IOutputWriter m_outputWriter;
        private readonly IExceptionThrowerProvider m_exceptionThrowerProvider;
        private readonly Lazy<IFrcSettingsProvider> m_frcSettingsProvider;
        private readonly int? m_inputTeamNumber;

        public DotNetTeamNumberProvider(int? inputTeamNumber, IOutputWriter outputWriter,
            IExceptionThrowerProvider exceptionThrowerProvider, 
            Lazy<IFrcSettingsProvider> frcSettingsProvider)
        {
            m_inputTeamNumber = inputTeamNumber;
            m_outputWriter = outputWriter;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_frcSettingsProvider = frcSettingsProvider;
        }

        public async Task<int> GetTeamNumberAsync()
        {   
            int teamNumber = -1;
            if (m_inputTeamNumber != null)
            {
                teamNumber = m_inputTeamNumber.Value;
                if (teamNumber < 0)
                {
                    await m_outputWriter.WriteLineAsync("Entered team number is not valid. Attempting to read from settings file").ConfigureAwait(false);
                }
            }
            if (teamNumber < 0)
            {
                var frcSettings = await m_frcSettingsProvider.Value.GetFrcSettingsAsync().ConfigureAwait(false);
                if (frcSettings == null)
                {
                    throw m_exceptionThrowerProvider.ThrowException("Could not find team number. Please either use a -t argument, \nor run dotnet frc settings to set a permanent team number");
                }
                teamNumber = frcSettings.TeamNumber;
            }
            return teamNumber;
        }

        internal static int? GetTeamNumberFromCommandOption(int valTeamNumber)
        {
            return valTeamNumber < 0 ? null : (int?)valTeamNumber;
        }
    }
}
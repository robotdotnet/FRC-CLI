using System.Threading.Tasks;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;
using Nito.AsyncEx;

namespace dotnet_frc
{
    internal abstract class FrcSubCommandBase : DotNetSubCommandBase
    {
        internal CommandOption _teamOption;
        internal CommandOption _verboseOption;

        public static void SetupBaseOptions(FrcSubCommandBase command, bool useTeam = true)
        {
            command.HelpOption("-h|--help");

            if (useTeam)
            {
                command._teamOption = command.Option(
                    "-t|--team",
                    "Force a team",
                    CommandOptionType.SingleValue
                );
            }

            command._verboseOption = command.Option(
                "-v|--verbose <VERBOSE>",
                "Verbose output",
                CommandOptionType.NoValue
            );

        }
    }
}

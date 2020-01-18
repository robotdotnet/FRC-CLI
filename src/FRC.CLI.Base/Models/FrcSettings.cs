using Newtonsoft.Json;
using System.Collections.Generic;

namespace FRC.CLI.Base.Models
{
    public class FrcSettings
    {
        public string TeamNumber { get; set; }
        public List<string> CommandLineArguments { get; }
        public List<string> DeployIgnoreFiles { get; }

        [JsonConstructor]
        public FrcSettings(string teamNumber, List<string> commandLineArguments, List<string> deployIgnoreFiles)
        {
            this.TeamNumber = teamNumber;
            this.CommandLineArguments = commandLineArguments;
            this.DeployIgnoreFiles = deployIgnoreFiles;
        }
    }
}
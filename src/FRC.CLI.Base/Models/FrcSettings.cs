using Newtonsoft.Json;
using System.Collections.Generic;

namespace FRC.CLI.Base.Models
{
    public class FrcSettings
    {
        [JsonProperty("teamNumber")]
        public int TeamNumber { get; set; }
        [JsonProperty("commandLineArguments")]
        public List<string> CommandLineArguments { get; }
        [JsonProperty("deployIgnoreFile")]
        public List<string> DeployIgnoreFiles { get; }

        [JsonConstructor]
        public FrcSettings(int teamNumber, List<string> commandLineArguments, List<string> deployIgnoreFiles)
        {
            this.TeamNumber = teamNumber;
            this.CommandLineArguments = commandLineArguments;
            this.DeployIgnoreFiles = deployIgnoreFiles;
        }
    }
}
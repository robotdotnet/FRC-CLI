using System.Collections.Generic;

namespace FRC.CLI.Base.Models
{
    public class FrcSettings
    {
        public string TeamNumber { get; set; }
        public List<string> CommandLineArguments { get; set; }
        public List<string> DeployIgnoreFiles { get; set; }
    }
}
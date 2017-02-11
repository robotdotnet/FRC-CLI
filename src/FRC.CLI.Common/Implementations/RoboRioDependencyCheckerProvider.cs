using System;
using System.Threading.Tasks;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class RoboRioDependencyCheckerProvider : IRoboRioDependencyCheckerProvider
    {
        public Task<bool> CheckIfDependenciesAreSatisfied()
        {
            return Task.FromResult(true);
        }
    }
}
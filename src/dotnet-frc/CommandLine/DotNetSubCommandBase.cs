// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.DotNet.Cli.CommandLine;

namespace Microsoft.DotNet.Cli
{
    internal abstract class DotNetSubCommandBase : CommandLineApplication
    {
        internal DotNetSubCommandBase() : base(throwOnUnexpectedArg: false)
        {
        }

        public abstract Task<int> RunAsync(string fileOrDirectory);
    }
}

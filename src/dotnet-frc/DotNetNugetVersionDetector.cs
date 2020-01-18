using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace dotnet_frc
{
    public class DotNetNugetVersionDetector
    {
        public static async Task<string?> FindNewestPackageVersion(string package, bool includePrerelease)
        {
            Logger logger = new Logger();
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support
            //providers.AddRange(Repository.Provider.GetCoreV2());  // Add v2 API support
            PackageSource packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            SourceRepository sourceRepository = new SourceRepository(packageSource, providers);
            PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
            IEnumerable<IPackageSearchMetadata> searchMetadata = await packageMetadataResource.GetMetadataAsync(package, true, true, logger, CancellationToken.None);
            NuGetVersion? newest = null;
            foreach(var item in searchMetadata)
            {
                if (item is PackageSearchMetadata itemMetadata)
                {
                    var version = itemMetadata.Version;
                    if (!itemMetadata.IsListed) continue;
                    if (version.IsPrerelease && !includePrerelease) continue;
                    if (newest == null)
                    {
                        newest = version;
                        continue;
                    }
                    if (version.CompareTo(newest.Version) > 0)
                    {
                        newest = version;
                    }
                }
            }
            if (newest == null) return null;
            return newest.ToNormalizedString();
        }
    }

    public class Logger : ILogger
{
  public void LogDebug(string data) => Console.WriteLine($"DEBUG: {data}");
  public void LogVerbose(string data) => Console.WriteLine($"VERBOSE: {data}");
  public void LogInformation(string data) => Console.WriteLine($"INFORMATION: {data}");
  public void LogMinimal(string data) => Console.WriteLine($"MINIMAL: {data}");
  public void LogWarning(string data) => Console.WriteLine($"WARNING: {data}");
  public void LogError(string data) => Console.WriteLine($"ERROR: {data}");
  public void LogSummary(string data) => Console.WriteLine($"SUMMARY: {data}");

        public void LogInformationSummary(string data)
        {

        }

        public void LogErrorSummary(string _)
        {
        }

    public void Log(LogLevel level, string data)
    {
      throw new NotImplementedException();
    }

    public Task LogAsync(LogLevel level, string data)
    {
      throw new NotImplementedException();
    }

    public void Log(ILogMessage message)
    {
      throw new NotImplementedException();
    }

    public Task LogAsync(ILogMessage message)
    {
      throw new NotImplementedException();
    }
  }
}

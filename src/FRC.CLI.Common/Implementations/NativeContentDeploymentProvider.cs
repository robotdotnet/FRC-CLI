using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using Newtonsoft.Json;

namespace FRC.CLI.Common.Implementations
{
    public class NativeContentDeploymentProvider : INativeContentDeploymentProvider
    {
        IWPILibNativeDeploySettingsProvider m_wpilibNativeDeploySettingsProvider;
        IProjectInformationProvider m_projectInformationProvider;
        IExceptionThrowerProvider m_exceptionThrowerProvider;
        IFileDeployerProvider m_fileDeployerProvider;
        IOutputWriter m_outputWriter;

        public const string NativeDirectoryFolder = "wpinative";

        public string NativeDirectory => NativeDirectoryFolder;

        public NativeContentDeploymentProvider(IWPILibNativeDeploySettingsProvider wpilibNativeDeploySettingsProvider,
            IProjectInformationProvider projectInformationProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IFileDeployerProvider fileDeployerProvider, IOutputWriter outputWriter,
            IMd5HashCheckerProvider md5HashFileChecker)
        {
            m_wpilibNativeDeploySettingsProvider = wpilibNativeDeploySettingsProvider;
            m_projectInformationProvider = projectInformationProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_fileDeployerProvider = fileDeployerProvider;
            m_outputWriter = outputWriter;
        }
        
        public IEnumerable<(string file, string hash)> GetFilesToUpdate(Stream inputFileStream, IEnumerable<(string file, string hash)> localFiles)
        {
            return GetFilesToUpdate(ReadFilesFromStream(inputFileStream), localFiles);
        }

        public IEnumerable<(string file, string hash)> GetFilesToUpdate(IEnumerable<(string file, string hash)> remoteFiles, 
            IEnumerable<(string file, string hash)> localFiles)
        {
            return localFiles.Where(x => !remoteFiles.Any(z => x.file == z.file && x.hash == z.hash));
        }

        public virtual IEnumerable<(string file, string hash)> ReadFilesFromStream(Stream inputFileStream)
        {
            using (StreamReader reader = new StreamReader(inputFileStream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer serializer = new JsonSerializer();
                try
                {
                    var retVal = serializer.Deserialize<List<(string, string)>>(jsonReader);
                    if (retVal == null)
                    {
                        return new List<(string, string)>();
                    }
                    return retVal;
                }
                catch (JsonReaderException)
                {
                    return new List<(string, string)>();
                }
            }
        }

        public async Task DeployNativeContentAsync()
        {
            await m_outputWriter.WriteLineAsync("Deploying native dependencies").ConfigureAwait(false);

            string buildPath = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);
            string nativeLoc = Path.Combine(buildPath, NativeDirectory);
            var fileMd5List = await GetMd5ForFilesAsync(nativeLoc, m_wpilibNativeDeploySettingsProvider.NativeIgnoreFiles).ConfigureAwait(false);

            if (fileMd5List == null)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to find all files to deploy");
            }

            MemoryStream memStream = new MemoryStream();
            bool readFile = await m_fileDeployerProvider.ReceiveFileAsync(
                $"{m_wpilibNativeDeploySettingsProvider.NativeDeployLocation}/{m_wpilibNativeDeploySettingsProvider.NativePropertiesFileName}",
                memStream, ConnectionUser.Admin).ConfigureAwait(false);
            if (!readFile)
            {
                // TODO: Add Verbose
                await DeployNativeLibrariesAsync(fileMd5List).ConfigureAwait(false);
                await m_outputWriter.WriteLineAsync("Successfully deployed native files").ConfigureAwait(false);
                return;
            }

            memStream.Position = 0;
            var updateList = GetFilesToUpdate(memStream, fileMd5List).ToList();

            if (updateList.Count == 0)
            {
                await m_outputWriter.WriteLineAsync("Native libraries already exist. Skipping").ConfigureAwait(false);
                return;
            }

            await DeployNativeLibrariesAsync(updateList).ConfigureAwait(false);
            await m_outputWriter.WriteLineAsync("Successfully deployed native files").ConfigureAwait(false);            
        }

        public virtual IEnumerable<string> GetNativeFileList(string fileLocation)
        {
            if (!Directory.Exists(fileLocation))
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to find native content directory to deploy");
            }

            return Directory.GetFiles(fileLocation);
        }

        public virtual async Task<List<(string file, string hash)>> GetMd5ForFilesAsync(string fileLocation, 
            IEnumerable<string> ignoreFiles)
        {
            var files = GetNativeFileList(fileLocation);

            var filtered = files.Where(x => !ignoreFiles.Contains(Path.GetFileName(x)));

            List<(string file, string hash)> retFiles = new List<(string, string)>();
            
            foreach (var file in filtered)
            {
                string hash = await MD5Helper.Md5SumAsync(file).ConfigureAwait(false);
                retFiles.Add((file, hash));
            }

            return retFiles;
        }

        public virtual string SerializeFileHashList(IEnumerable<(string file, string hash)> files)
        {
            return JsonConvert.SerializeObject(files, Formatting.Indented);
        }

        public async Task DeployNativeFilesAsync(IEnumerable<string> files)
        {
            bool nativeDeploy = await m_fileDeployerProvider.DeployFilesAsync(files.Select(x => (x, m_wpilibNativeDeploySettingsProvider.NativeDeployLocation)), 
                ConnectionUser.Admin).ConfigureAwait(false);
            await m_fileDeployerProvider.RunCommandAsync("ldconfig", ConnectionUser.Admin).ConfigureAwait(false);

            if (!nativeDeploy)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to deploy native files");
            }
        }

        public async Task DeployNativeLibrariesAsync(IEnumerable<(string file, string hash)> files)
        {
            var jsonString = SerializeFileHashList(files);

            MemoryStream memStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memStream);
            await writer.WriteAsync(jsonString).ConfigureAwait(false);
            writer.Flush();
            memStream.Position = 0;

            await DeployNativeFilesAsync(files.Select(x => x.file)).ConfigureAwait(false);;

            var md5Deploy = await m_fileDeployerProvider.DeployStreamAsync(memStream, 
                $"{m_wpilibNativeDeploySettingsProvider.NativeDeployLocation}/{m_wpilibNativeDeploySettingsProvider.NativePropertiesFileName}", 
                ConnectionUser.Admin).ConfigureAwait(false);

            if (!md5Deploy)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to deploy native files");
            }
        }
    }
}
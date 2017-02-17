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

        public string NativeDirectory => "wpinative";

        public NativeContentDeploymentProvider(IWPILibNativeDeploySettingsProvider wpilibNativeDeploySettingsProvider,
            IProjectInformationProvider projectInformationProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IFileDeployerProvider fileDeployerProvider, IOutputWriter outputWriter)
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
                var retVal = serializer.Deserialize<List<(string file, string hash)>>(jsonReader);
                if (retVal == null)
                {
                    return new List<(string, string)>();
                }
                return retVal;
            }
        }

        

        public async Task DeployNativeContentAsync()
        {
            await m_outputWriter.WriteLineAsync("Deploying native dependencies").ConfigureAwait(false);

            string buildPath = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);
            string nativeLoc = Path.Combine(buildPath, NativeDirectory);
            var fileMd5List = await GetMd5ForFilesAsync(nativeLoc, m_wpilibNativeDeploySettingsProvider.NativeIgnoreFiles);

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

            var updateList = GetFilesToUpdate(memStream, fileMd5List).ToList();

            if (updateList.Count == 0)
            {
                await m_outputWriter.WriteLineAsync("Native libraries already exist. Skipping").ConfigureAwait(false);
                return;
            }

            await DeployNativeLibrariesAsync(fileMd5List).ConfigureAwait(false);
            await m_outputWriter.WriteLineAsync("Successfully deployed native files").ConfigureAwait(false);            
        }

        public virtual async Task<List<(string file, string hash)>> GetMd5ForFilesAsync(string fileLocation, 
            IEnumerable<string> ignoreFiles)
        {
            if (Directory.Exists(fileLocation))
            {
                string[] files = Directory.GetFiles(fileLocation);
                List<(string file, string hash)> retList = new List<(string file, string hash)>(files.Length);
                foreach (var file in files)
                {
                    bool skip = false;
                    foreach (string ignoreFile in ignoreFiles)
                    {
                        if (file.Contains(ignoreFile))
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip) continue;
                    string md5 = await MD5Helper.Md5SumAsync(file);
                    retList.Add((file, md5));
                }
                return retList;
            }
            else
            {
                return null;
            }
        }

        public async Task DeployNativeLibrariesAsync(IList<(string file, string hash)> files)
        {
            bool nativeDeploy = false;

            string tempFile = Path.Combine(Path.GetTempPath(), m_wpilibNativeDeploySettingsProvider.NativePropertiesFileName);

            await Task.Run(() => 
            {
                var json = JsonConvert.SerializeObject(files, Formatting.Indented);
                File.WriteAllText(tempFile, json);
            });

            nativeDeploy = await m_fileDeployerProvider.DeployFilesAsync(files.Select(x => x.file), 
                m_wpilibNativeDeploySettingsProvider.NativeDeployLocation, ConnectionUser.Admin).ConfigureAwait(false);
            // TODO: Figure out why trying to deploy the raw MemoryStream was Deadlocking
            //md5Deploy = await rioConn.DeployFileAsync(memStream, $"{remoteDirectory}/{propertiesName}32.properties", ConnectionUser.Admin).ConfigureAwait(false);
            await m_fileDeployerProvider.RunCommandAsync("ldconfig", ConnectionUser.Admin).ConfigureAwait(false);

            try
            {
                File.Delete(tempFile);
            }
            catch (Exception)
            {

            }

            if (!nativeDeploy)
            {
                throw m_exceptionThrowerProvider.ThrowException("Failed to deploy native files");
            }
        }
    }
}
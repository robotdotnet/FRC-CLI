using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;

namespace FRC.CLI.Common.Implementations
{
    public class NativePackageDeploymentProvider : INativePackageDeploymentProvider
    {
        IWPILibNativeDeploySettingsProvider m_wpilibNativeDeploySettingsProvider;
        IProjectInformationProvider m_projectInformationProvider;
        IExceptionThrowerProvider m_exceptionThrowerProvider;
        IFileDeployerProvider m_fileDeployerProvider;

        public string NativeDirectory => "wpinative";

        public NativePackageDeploymentProvider(IWPILibNativeDeploySettingsProvider wpilibNativeDeploySettingsProvider,
            IProjectInformationProvider projectInformationProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            IFileDeployerProvider fileDeployerProvider)
        {
            m_wpilibNativeDeploySettingsProvider = wpilibNativeDeploySettingsProvider;
            m_projectInformationProvider = projectInformationProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_fileDeployerProvider = fileDeployerProvider;
        }

        public async Task<bool> DeployNativeFilesAsync()
        {
            MemoryStream memStream = new MemoryStream();
            bool readFile = await m_fileDeployerProvider.ReceiveFileAsync(
                $"{m_wpilibNativeDeploySettingsProvider.NativeDeployLocation}/{m_wpilibNativeDeploySettingsProvider.NativePropertiesFileName}",
                memStream, ConnectionUser.Admin).ConfigureAwait(false);
            
            string buildPath = await m_projectInformationProvider.GetProjectBuildDirectoryAsync().ConfigureAwait(false);
            string nativeLoc = Path.Combine(buildPath, NativeDirectory);

            var fileMd5List = await GetMd5ForFilesAsync(nativeLoc, m_wpilibNativeDeploySettingsProvider.NativeIgnoreFiles);

            if (fileMd5List == null)
            {
                return false;
            }
            if (!readFile)
            {
                return await DeployNativeLibrariesAsync(fileMd5List).ConfigureAwait(false);
            }

            memStream.Position = 0;

            bool foundError = false;
            int readCount = 0;
            using (StreamReader reader = new StreamReader(memStream))
            {
                string line = null;
                while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
                {
                    // Split line at =
                    string[] split = line.Split('=');
                    if (split.Length < 2) continue;
                    readCount++;
                    foreach (Tuple<string, string> tuple in fileMd5List)
                    {
                        if (split[0] == tuple.Item1)
                        {
                            // Found a match file name
                            if (split[1] != tuple.Item2)
                            {
                                foundError = true;
                            }
                            break;
                        }
                    }
                    if (foundError) break;
                }
            }

            if (foundError || readCount != fileMd5List.Count)
            {
                return await DeployNativeLibrariesAsync(fileMd5List).ConfigureAwait(false);
            }


            return true;
        }

        public async Task<List<Tuple<string, string>>> GetMd5ForFilesAsync(string fileLocation, IList<string> ignoreFiles)
        {
            if (Directory.Exists(fileLocation))
            {
                string[] files = Directory.GetFiles(fileLocation);
                List<Tuple<string, string>> retList = new List<Tuple<string, string>>(files.Length);
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
                    retList.Add(new Tuple<string, string>(file, md5));
                }
                return retList;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeployNativeLibrariesAsync(List<Tuple<string, string>> files)
        {
            List<string> fileList = new List<string>(files.Count);
            bool nativeDeploy = false;

            string tempFile = Path.Combine(Path.GetTempPath(), m_wpilibNativeDeploySettingsProvider.NativePropertiesFileName);

            using (FileStream memStream = new FileStream(tempFile, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(memStream))
            {
                foreach (Tuple<string, string> tuple in files)
                {
                    fileList.Add(tuple.Item1);
                    await writer.WriteLineAsync($"{tuple.Item1}={tuple.Item2}").ConfigureAwait(false);
                }

                writer.Flush();

                fileList.Add(tempFile);
            }

            nativeDeploy = await m_fileDeployerProvider.DeployFilesAsync(fileList, 
                m_wpilibNativeDeploySettingsProvider.NativeDeployLocation, ConnectionUser.Admin).ConfigureAwait(false);
            // TODO: Figure out why trying to deploy the raw MemoryStream was Deadlocking
            //md5Deploy = await rioConn.DeployFileAsync(memStream, $"{remoteDirectory}/{propertiesName}32.properties", ConnectionUser.Admin).ConfigureAwait(false);
            await m_fileDeployerProvider.RunCommandsAsync(new string[] {"ldconfig"}, ConnectionUser.Admin).ConfigureAwait(false);

            try
            {
                File.Delete(tempFile);
            }
            catch (Exception)
            {

            }

            return nativeDeploy;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common.Connections;

namespace FRC.CLI.Common
{
    public class CachedFileHelper
    {
        IFileDeployer m_fileDeployer;
        IOutputWriter m_outputWriter;

        public CachedFileHelper(IOutputWriter outputWriter, IFileDeployer fileDeployer)
        {
            m_outputWriter = outputWriter;
            m_fileDeployer = fileDeployer;
        }

        public async Task<bool> CheckAndDeployNativeLibrariesAsync(string remoteDirectory, string propertiesName, string dirToUpload, IList<string> ignoreFiles)
        {
            MemoryStream stream = new MemoryStream();
            bool readFile = await m_fileDeployer.ReceiveFileAsync($"{remoteDirectory}/{propertiesName}.properties", stream,
                            ConnectionUser.LvUser).ConfigureAwait(false);

            string nativeLoc = dirToUpload;

            var fileMd5List = GetMD5ForFiles(nativeLoc, ignoreFiles);
            if (fileMd5List == null)
            {
                await
                    m_outputWriter.WriteLineAsync(
                            "Native libraries cannot be found. Please see our troubleshooting site (https://robotdotnet.github.io/articles/troubleshooting.html) for instructions for fixing this.")
                        .ConfigureAwait(false);
                return false;
            }
            if (!readFile)
            {
                // Libraries definitely do not exist, deploy
                return await DeployNativeLibrariesAsync(fileMd5List, remoteDirectory, propertiesName).ConfigureAwait(false);
            }

            stream.Position = 0;

            bool foundError = false;
            int readCount = 0;
            StreamReader reader = new StreamReader(stream);
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

            reader.Dispose();

            if (foundError || readCount != fileMd5List.Count)
            {
                return await DeployNativeLibrariesAsync(fileMd5List, remoteDirectory, propertiesName).ConfigureAwait(false);
            }

            await m_outputWriter.WriteLineAsync("Native libraries exist. Skipping deploy").ConfigureAwait(false);
            return true;
        }

        public List<Tuple<string, string>> GetMD5ForFiles(string fileLocation, IList<string> ignoreFiles)
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
                    string md5 = MD5Helper.Md5Sum(file);
                    retList.Add(new Tuple<string, string>(file, md5));
                }
                return retList;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeployNativeLibrariesAsync(List<Tuple<string, string>> files, string remoteDirectory, string propertiesName)
        {
            List<string> fileList = new List<string>(files.Count);
            bool nativeDeploy = false;

            string tempFile = Path.Combine(Path.GetTempPath(), $"{propertiesName}.properties");

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

            await m_outputWriter.WriteLineAsync("Deploying native files").ConfigureAwait(false);
            nativeDeploy = await m_fileDeployer.DeployFilesAsync(fileList, remoteDirectory, ConnectionUser.Admin).ConfigureAwait(false);
            // TODO: Figure out why trying to deploy the raw MemoryStream was Deadlocking
            //md5Deploy = await rioConn.DeployFileAsync(memStream, $"{remoteDirectory}/{propertiesName}32.properties", ConnectionUser.Admin).ConfigureAwait(false);
            await m_fileDeployer.RunCommandsAsync(new string[] {"ldconfig" }, ConnectionUser.Admin).ConfigureAwait(false);

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using FRC.CLI.Base.Interfaces;
using Nito.AsyncEx;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace FRC.CLI.Common.Connections
{
    public class RoboRioConnection : IFileDeployerProvider
    {
        public const string RoboRioMdnsFormatString = "roborio-{0}-FRC.local";
        public const string RoboRioLanFormatString = "roborio-{0}-FRC.lan";
        public const string RoboRioUSBIp = "172.22.11.2";
        public const string RoboRioIpFormatString = "10.{0}.{1}.2";

        private ITeamNumberProvider m_teamNumberProvider;
        private IPAddress m_remoteIp;
        private TimeSpan m_sshTimeout;
        
        private ConnectionInfo m_adminConnectionInfo;
        private ConnectionInfo m_lvUserConnectionInfo;

        private SshClient m_sshAdminClient;
        private SshClient m_sshUserClient;
        private ScpClient m_scpUserClient;
        private ScpClient m_scpAdminClient;

        private IOutputWriter m_outputWriter;
        private IBuildSettingsProvider m_buildSettingsProvider;
        private IExceptionThrowerProvider m_exceptionThrowerProvider;


        public bool Connected => m_remoteIp != null;

        public RoboRioConnection(IOutputWriter outputWriter,
            IBuildSettingsProvider buildSettingsProvider, IExceptionThrowerProvider exceptionThrowerProvider,
            ITeamNumberProvider teamNumberProvider)
        {
            m_teamNumberProvider = teamNumberProvider;
            m_sshTimeout = TimeSpan.FromSeconds(2);
            m_outputWriter = outputWriter;
            m_buildSettingsProvider = buildSettingsProvider;
            m_exceptionThrowerProvider = exceptionThrowerProvider;
            m_remoteIp = null;
        }

        private void CreateConnection()
        {
            if (Connected)
            {
                return;
            }
            AsyncContext.Run(async () =>
            {
                await CreateConnectionAsync().ConfigureAwait(false);;
            });
        }

        private async Task CreateConnectionAsync()
        {
            if (Connected)
            {
                return;
            }

            int teamNumber = await m_teamNumberProvider.GetTeamNumberAsync().ConfigureAwait(false);

            if (teamNumber < 0)
            {
                throw m_exceptionThrowerProvider.ThrowException("Team number cannot be less than 0. Check settings");
            }

            await m_outputWriter.WriteLineAsync($"Connecting to robot for team {teamNumber}").ConfigureAwait(false);

            string roboRioMDNS = string.Format(RoboRioMdnsFormatString, teamNumber);
            string roboRioLan = string.Format(RoboRioLanFormatString, teamNumber);
            string roboRIOIP = string.Format(RoboRioIpFormatString, teamNumber / 100, teamNumber % 100);

            bool verbose = m_buildSettingsProvider.Verbose;
            if (verbose)
            {
                await m_outputWriter.WriteLineAsync($"Connecting to the following IP Addresses:").ConfigureAwait(false);
                await m_outputWriter.WriteLineAsync($"    {RoboRioUSBIp}").ConfigureAwait(false);
                await m_outputWriter.WriteLineAsync($"    {roboRioMDNS}").ConfigureAwait(false);
                await m_outputWriter.WriteLineAsync($"    {roboRioLan}").ConfigureAwait(false);
                await m_outputWriter.WriteLineAsync($"    {roboRIOIP}").ConfigureAwait(false);
            }

            using (TcpClient usbClient = new TcpClient())
            using (TcpClient mDnsClient = new TcpClient())
            using (TcpClient lanClient = new TcpClient())
            using (TcpClient ipClient = new TcpClient())
            {

                Task usb = usbClient.ConnectAsync(RoboRioUSBIp, 80);
                Task mDns = mDnsClient.ConnectAsync(roboRioMDNS, 80);
                Task lan = lanClient.ConnectAsync(roboRioLan, 80);
                Task ip = ipClient.ConnectAsync(roboRIOIP, 80);
                Task delayTask = Task.Delay(m_sshTimeout);

                // http://stackoverflow.com/questions/24441474/await-list-of-async-predicates-but-drop-out-on-first-false
                List<Task> tasks = new List<Task>()
                {
                    usb, mDns, lan, ip, delayTask
                };

                while (tasks.Count != 0)
                {
                    var finished = await Task.WhenAny(tasks);

                    if (finished == delayTask)
                    {
                        throw m_exceptionThrowerProvider.ThrowException("Connection to robot timed out");
                    }
                    else if (finished.IsCompleted && !finished.IsFaulted && !finished.IsCanceled)
                    {
                        // A task finished, find our host
                        TcpClient foundHost = null;
                        if (finished == usb)
                        {
                            foundHost = usbClient;
                        }
                        else if (finished == mDns)
                        {
                            foundHost = mDnsClient;
                        }
                        else if (finished == lan)
                        {
                            foundHost = lanClient;
                        }
                        else if (finished == ip)
                        {
                            foundHost = ipClient;
                        }
                        else
                        {
                            // Error
                            throw m_exceptionThrowerProvider.ThrowException("Unknown task returned");
                        }

                        var ep = foundHost.Client.RemoteEndPoint;
                        var ipEp = ep as IPEndPoint;
                        if (ipEp == null)
                        {
                            // Continue, we cannot use this one

                        }
                        else
                        {
                            bool finishedConnect = await OnConnectionFound(ipEp.Address);
                            if (finishedConnect)
                            {
                                await m_outputWriter.WriteLineAsync($"Connected to robot at IP Address: {ipEp.Address}");
                                m_remoteIp = ipEp.Address;
                                return;
                            }
                            else 
                            {
                                throw m_exceptionThrowerProvider.ThrowException("Failed to complete all RoboRio connections");
                            }
                        }
                    }
                    tasks.Remove(finished);
                }
                // If we have ever gotten here, return false
                throw m_exceptionThrowerProvider.ThrowException("Ran out of tasks");
            }
        }

        private async Task<bool> OnConnectionFound(IPAddress ip)
        {
            //User auth method
            KeyboardInteractiveAuthenticationMethod authMethod = new KeyboardInteractiveAuthenticationMethod("lvuser");
            PasswordAuthenticationMethod pauth = new PasswordAuthenticationMethod("lvuser", "");

            authMethod.AuthenticationPrompt += (sender, e) =>
            {
                foreach (
                    AuthenticationPrompt p in
                        e.Prompts.Where(
                            p => p.Request.IndexOf("Password:") != -1))
                {
                    p.Response = "";
                }
            };

            //Admin Auth Method
            KeyboardInteractiveAuthenticationMethod authMethodAdmin = new KeyboardInteractiveAuthenticationMethod("admin");
            PasswordAuthenticationMethod pauthAdmin = new PasswordAuthenticationMethod("admin", "");

            authMethodAdmin.AuthenticationPrompt += (sender, e) =>
            {
                foreach (
                    AuthenticationPrompt p in
                        e.Prompts.Where(
                            p => p.Request.IndexOf("Password:") != -1))
                {
                    p.Response = "";
                }
            };

            m_lvUserConnectionInfo = new ConnectionInfo(ip.ToString(), "lvuser", pauth, authMethod) { Timeout = m_sshTimeout };


            m_adminConnectionInfo = new ConnectionInfo(ip.ToString(), "admin", pauthAdmin, authMethodAdmin) { Timeout = m_sshTimeout };

            bool verbose = m_buildSettingsProvider.Verbose;

            if (verbose)
                await m_outputWriter.WriteLineAsync("Creating lvuser ssh client");
            try
            {
                m_sshUserClient = new SshClient(m_lvUserConnectionInfo);
                await Task.Run(() => m_sshUserClient.Connect()).ConfigureAwait(false);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (SshOperationTimeoutException)
            {
                return false;
            }

            if (verbose)
                await m_outputWriter.WriteLineAsync("Creating lvuser scp client");

            try
            {
                m_scpUserClient = new ScpClient(m_lvUserConnectionInfo);
                await Task.Run(() => m_scpUserClient.Connect()).ConfigureAwait(false);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (SshOperationTimeoutException)
            {
                return false;
            }

            if (verbose)
                await m_outputWriter.WriteLineAsync("Creating admin ssh client");

            try
            {
                m_sshAdminClient = new SshClient(m_adminConnectionInfo);
                await Task.Run(() => m_sshAdminClient.Connect()).ConfigureAwait(false);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (SshOperationTimeoutException)
            {
                return false;
            }

            if (verbose)
                await m_outputWriter.WriteLineAsync("Creating admin scp client");

            try
            {
                m_scpAdminClient = new ScpClient(m_adminConnectionInfo);
                await Task.Run(() => m_scpAdminClient.Connect()).ConfigureAwait(false);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (SshOperationTimeoutException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeployFilesAsync(IEnumerable<string> files, string deployLocation, ConnectionUser user)
        {
            await CreateConnectionAsync().ConfigureAwait(false);
            ScpClient scp;
            switch (user)
            {
                case ConnectionUser.Admin:
                    scp = m_scpAdminClient;
                    break;
                case ConnectionUser.LvUser:
                    scp = m_scpUserClient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(user), user, null);
            }

            bool verbose = m_buildSettingsProvider.Verbose;
            foreach (FileInfo fileInfo in from string s in files where File.Exists(s) select new FileInfo(s))
            {
                if (verbose)
                {
                    await m_outputWriter.WriteLineAsync($"Deploying File: {fileInfo.Name}").ConfigureAwait(false);
                }
                await Task.Run(() => scp.Upload(fileInfo, deployLocation)).ConfigureAwait(false);
            }
            return true;
        }

        public async Task<bool> ReceiveFileAsync(string remoteFile, Stream receiveStream, ConnectionUser user)
        {
            await CreateConnectionAsync().ConfigureAwait(false);
            ScpClient scp;
            switch (user)
            {
                case ConnectionUser.Admin:
                    scp = m_scpAdminClient;
                    break;
                case ConnectionUser.LvUser:
                    scp = m_scpUserClient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(user), user, null);
            }

            if (receiveStream == null || !receiveStream.CanWrite)
            {
                return false;
            }

            bool verbose = m_buildSettingsProvider.Verbose;
            if (verbose)
            {
                await m_outputWriter.WriteLineAsync($"Receiving File: {remoteFile}").ConfigureAwait(false);
            }
            try
            {
                var ret = await Task.Run(() => 
                {
                    try
                    {
                        scp.Download(remoteFile, receiveStream);
                        return true;
                    }
                    catch (ScpException)
                    {
                        return false;
                    }
                }).ConfigureAwait(false);
                return ret;
            }
            catch (ScpException)
            {
                return false;
            }
            catch (SshException)
            {
                return false;
            }
        }

        public async Task<Dictionary<string, SshCommand>> RunCommandsAsync(IList<string> commands, ConnectionUser user)
        {
            await CreateConnectionAsync().ConfigureAwait(false);
            SshClient ssh;
            switch (user)
            {
                case ConnectionUser.Admin:
                    ssh = m_sshAdminClient;
                    break;
                case ConnectionUser.LvUser:
                    ssh = m_sshUserClient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(user), user, null);
            }

            Dictionary<string, SshCommand> retCommands = new Dictionary<string, SshCommand>();

            bool verbose = m_buildSettingsProvider.Verbose;
            foreach (string s in commands)
            {
                if (verbose)
                {
                    await m_outputWriter.WriteLineAsync($"Running command: {s}").ConfigureAwait(false);
                }
                await Task.Run(() =>
                {
                    var x = ssh.RunCommand(s);

                    retCommands.Add(s, x);
                }).ConfigureAwait(false);
            }
            return retCommands;
        }

        public async Task<SshCommand> RunCommandAsync(string command, ConnectionUser user)
        {
            await CreateConnectionAsync().ConfigureAwait(false);
            SshClient ssh;
            switch (user)
            {
                case ConnectionUser.Admin:
                    ssh = m_sshAdminClient;
                    break;
                case ConnectionUser.LvUser:
                    ssh = m_sshUserClient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(user), user, null);
            }

            Dictionary<string, SshCommand> retCommands = new Dictionary<string, SshCommand>();

            bool verbose = m_buildSettingsProvider.Verbose;
            if (verbose)
            {
                await m_outputWriter.WriteLineAsync($"Running command: {command}").ConfigureAwait(false);
            }
            return await Task.Run(() =>
            {
                var x = ssh.RunCommand(command);
                return x;
            }).ConfigureAwait(false);
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    m_scpAdminClient?.Dispose();
                    m_sshUserClient?.Dispose();
                    m_scpUserClient?.Dispose();
                    m_scpAdminClient?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RoboRioConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public async Task<IPAddress> GetConnectionIpAsync()
        {
            await CreateConnectionAsync().ConfigureAwait(false);
            return m_remoteIp;
        }

        public IPAddress GetConnectionIp()
        {
            CreateConnection();
            return m_remoteIp;
        }
        #endregion

    }
}
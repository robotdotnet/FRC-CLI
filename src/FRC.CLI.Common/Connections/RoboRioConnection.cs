using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private int m_teamNumber;
        private IPAddress m_remoteIp;
        private TimeSpan m_sshTimeout;
        
        private ConnectionInfo m_adminConnectionInfo;
        private ConnectionInfo m_lvUserConnectionInfo;

        private SshClient m_sshAdminClient;
        private SshClient m_sshUserClient;
        private ScpClient m_scpUserClient;
        private ScpClient m_scpAdminClient;

        private IOutputWriter m_outputWriter;


        public bool Connected => m_remoteIp != null;

        public IPAddress ConnectionIp => m_remoteIp;

        public RoboRioConnection(int teamNumber, TimeSpan sshTimeout, IOutputWriter outputWriter)
        {
            m_teamNumber = teamNumber;
            m_sshTimeout = sshTimeout;
            m_outputWriter = outputWriter;
        }

        public static async Task<RoboRioConnection> StartConnectionTaskAsync(int teamNumber, IOutputWriter outputWriter)
        {
            var roboRioConnection = new RoboRioConnection(teamNumber, TimeSpan.FromSeconds(2), outputWriter);
            bool connected = await roboRioConnection.CreateConnectionAsync().ConfigureAwait(false);
            if (connected)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Connected to roboRIO...");
                builder.AppendLine($"IP Address: {roboRioConnection.ConnectionIp.ToString()}");
                await outputWriter.WriteLineAsync(builder.ToString()).ConfigureAwait(false);
            }
            else
            {
                await outputWriter.WriteLineAsync("Failed to connect to roboRIO...").ConfigureAwait(false);
            }
            return roboRioConnection;
        }

        public static RoboRioConnection StartConnectionTask(int teamNumber, IOutputWriter outputWriter)
        {
            var roboRioConnection = new RoboRioConnection(teamNumber, TimeSpan.FromSeconds(2), outputWriter);
            bool connected = roboRioConnection.CreateConnection();
            if (connected)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Connected to roboRIO...");
                builder.AppendLine($"IP Address: {roboRioConnection.ConnectionIp.ToString()}");
                outputWriter.WriteLine(builder.ToString());
            }
            else
            {
                outputWriter.WriteLine("Failed to connect to roboRIO...");
            }
            return roboRioConnection;
        }

        public bool CreateConnection()
        {
            return AsyncContext.Run(async () =>
            {
                return await CreateConnectionAsync().ConfigureAwait(false);;
            });
        }

        public async Task<bool> CreateConnectionAsync()
        {

            if (m_teamNumber < 0)
            {
                throw new InvalidOperationException("Team number cannot be less than 0");
            }

            string roboRioMDNS = string.Format(RoboRioMdnsFormatString, m_teamNumber);
            string roboRioLan = string.Format(RoboRioLanFormatString, m_teamNumber);
            string roboRIOIP = string.Format(RoboRioIpFormatString, m_teamNumber / 100, m_teamNumber % 100);

            using (TcpClient usbClient = new TcpClient())
            using (TcpClient mDnsClient = new TcpClient())
            using (TcpClient lanClient = new TcpClient())
            using (TcpClient ipClient = new TcpClient())
            {

                Task usb = usbClient.ConnectAsync(RoboRioUSBIp, 80);
                Task mDns = mDnsClient.ConnectAsync(roboRioMDNS, 80);
                Task lan = lanClient.ConnectAsync(roboRioLan, 80);
                Task ip = ipClient.ConnectAsync(roboRIOIP, 80);
                Task delayTask = Task.Delay(10000);

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
                        return false;
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
                            return false;
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
                                m_remoteIp = ipEp.Address;
                            }
                            return finishedConnect;
                        }
                    }
                    tasks.Remove(finished);
                }
                // If we have ever gotten here, return false
                return false;
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

            try
            {
                m_sshUserClient = new SshClient(m_lvUserConnectionInfo);
                await Task.Run(() => m_sshUserClient.Connect()).ConfigureAwait(false);
            }
            catch (SocketException e)
            {
                return false;
            }
            catch (SshOperationTimeoutException e )
            {
                return false;
            }

            try
            {
                m_scpUserClient = new ScpClient(m_lvUserConnectionInfo);
                await Task.Run(() => m_scpUserClient.Connect()).ConfigureAwait(false);
            }
            catch (SocketException e)
            {
                return false;
            }
            catch (SshOperationTimeoutException e)
            {
                return false;
            }

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

        public bool DeployFiles(IEnumerable<string> files, string deployLocation, ConnectionUser user)
        {
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

            bool verbose = true;
            foreach (FileInfo fileInfo in from string s in files where File.Exists(s) select new FileInfo(s))
            {
                if (verbose)
                {
                    m_outputWriter.WriteLine($"Deploying File: {fileInfo.Name}");
                }
                scp.Upload(fileInfo, deployLocation);
            }
            return true;
        }

        public async Task<bool> DeployFilesAsync(IEnumerable<string> files, string deployLocation, ConnectionUser user)
        {
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

            bool verbose = true;
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

        public bool ReceiveFile(string remoteFile, Stream receiveStream, ConnectionUser user)
        {
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

            bool verbose = true;
            if (verbose)
            {
                m_outputWriter.WriteLine($"Receiving File: {remoteFile}");
            }
            try
            {
                scp.Download(remoteFile, receiveStream);
            }
            catch (SshException)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> ReceiveFileAsync(string remoteFile, Stream receiveStream, ConnectionUser user)
        {
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

            bool verbose = true;
            if (verbose)
            {
                await m_outputWriter.WriteLineAsync($"Receiving File: {remoteFile}").ConfigureAwait(false);
            }
            try
            {
                await Task.Run(() => scp.Download(remoteFile, receiveStream)).ConfigureAwait(false);
            }
            catch (SshException)
            {
                return false;
            }
            return true;
        }

        public Dictionary<string, SshCommand> RunCommands(IList<string> commands, ConnectionUser user)
        {
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

            bool verbose = true;
            foreach (string s in commands)
            {
                if (verbose)
                {
                    m_outputWriter.WriteLine($"Running command: {s}");
                }
                var x = ssh.RunCommand(s);

                retCommands.Add(s, x);
            }
            return retCommands;
        }

        public async Task<Dictionary<string, SshCommand>> RunCommandsAsync(IList<string> commands, ConnectionUser user)
        {
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

            bool verbose = true;
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
        #endregion

    }
}
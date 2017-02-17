using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using Renci.SshNet;

namespace FRC.CLI.Base.Interfaces
{
    public interface IFileDeployerProvider : IDisposable
    {
        bool Connected { get; }
        Task<IPAddress> GetConnectionIpAsync();
        Task<Dictionary<string, SshCommand>> RunCommandsAsync(IList<string> commands, ConnectionUser user);
        Task<SshCommand> RunCommandAsync(string command, ConnectionUser user);
        Task<bool> ReceiveFileAsync(string remoteFile, Stream receiveStream, ConnectionUser user);
        Task<bool> DeployFilesAsync(IEnumerable<string> files, string deployLocation,
            ConnectionUser user);
        Task<bool> DeployStreamAsync(Stream stream, string deployLocation, ConnectionUser user);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FRC.CLI.Base.Enums;
using Renci.SshNet;

namespace FRC.CLI.Base.Interfaces
{
    public interface IFileDeployer : IDisposable
    {
        bool Connected { get; }
        IPAddress ConnectionIp { get; }
        Task<bool> CreateConnectionAsync();
        bool CreateConnection();
        Task<Dictionary<string, SshCommand>> RunCommandsAsync(IList<string> commands, ConnectionUser user);
        Dictionary<string, SshCommand> RunCommands(IList<string> commands, ConnectionUser user);
        Task<bool> ReceiveFileAsync(string remoteFile, Stream receiveStream, ConnectionUser user);
        bool ReceiveFile(string remoteFile, Stream receiveStream, ConnectionUser user);
        Task<bool> DeployFilesAsync(IEnumerable<string> files, string deployLocation,
            ConnectionUser user);
        bool DeployFiles(IEnumerable<string> files, string deployLocation, ConnectionUser user);
    }
}
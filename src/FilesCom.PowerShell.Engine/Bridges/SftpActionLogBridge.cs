using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SftpActionLog operations with the SDK and projects the SDK's SftpActionLog into the module's.
    internal static class SftpActionLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SftpActionLog Project(Sdk.SftpActionLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SftpActionLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                RemoteIp = source.RemoteIp,
                ServerIp = source.ServerIp,
                Username = source.Username,
                SshClientIdentification = source.SshClientIdentification,
                SessionUuid = source.SessionUuid,
                SeqId = source.SeqId,
                AuthMethod = source.AuthMethod,
                AuthCiphers = source.AuthCiphers,
                ActionType = source.ActionType,
                Path = source.Path,
                TruePath = source.TruePath,
                Name = source.Name,
                SftpResponseCode = source.SftpResponseCode,
                SftpResponseMessage = source.SftpResponseMessage,
                Md5 = source.Md5,
                Size = source.Size,
                DataLength = source.DataLength,
                BytesTransferred = source.BytesTransferred,
                EntriesReturned = source.EntriesReturned,
                Success = source.Success,
                Status = source.Status,
                DurationMs = source.DurationMs,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.SftpActionLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SftpActionLogs.List(parameters, requestOptions), source => SftpActionLogBridge.Project(source, connection, options));
        }
    }
}

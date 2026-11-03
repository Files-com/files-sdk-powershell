using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs FtpActionLog operations with the SDK and projects the SDK's FtpActionLog into the module's.
    internal static class FtpActionLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FtpActionLog Project(Sdk.FtpActionLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FtpActionLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                RemoteIp = source.RemoteIp,
                ServerIp = source.ServerIp,
                Username = source.Username,
                SessionUuid = source.SessionUuid,
                SeqId = source.SeqId,
                AuthCiphers = source.AuthCiphers,
                ActionType = source.ActionType,
                Path = source.Path,
                TruePath = source.TruePath,
                Name = source.Name,
                Cmd = source.Cmd,
                Param = source.Param,
                ResponseCode = source.ResponseCode,
                ResponseMessage = source.ResponseMessage,
                EntriesReturned = source.EntriesReturned,
                Success = source.Success,
                Status = source.Status,
                DurationMs = source.DurationMs,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.FtpActionLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.FtpActionLogs.List(parameters, requestOptions), source => FtpActionLogBridge.Project(source, connection, options));
        }
    }
}

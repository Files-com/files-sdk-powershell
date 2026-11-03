using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs InboundS3Log operations with the SDK and projects the SDK's InboundS3Log into the module's.
    internal static class InboundS3LogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.InboundS3Log Project(Sdk.InboundS3Log source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.InboundS3Log
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Path = source.Path,
                ClientIp = source.ClientIp,
                Operation = source.Operation,
                Status = source.Status,
                AwsAccessKey = source.AwsAccessKey,
                ErrorMessage = source.ErrorMessage,
                ErrorType = source.ErrorType,
                DurationMs = source.DurationMs,
                RequestId = source.RequestId,
                UserAgent = source.UserAgent,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.InboundS3Log> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.InboundS3Logs.List(parameters, requestOptions), source => InboundS3LogBridge.Project(source, connection, options));
        }
    }
}

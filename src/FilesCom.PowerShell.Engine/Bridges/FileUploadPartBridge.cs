using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's FileUploadPart into the module's.
    internal static class FileUploadPartBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FileUploadPart Project(Sdk.FileUploadPart source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FileUploadPart
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Send = Values.Json(source.Send),
                Action = source.Action,
                AskAboutOverwrites = source.AskAboutOverwrites,
                AvailableParts = source.AvailableParts,
                Expires = source.Expires,
                Headers = Values.Json(source.Headers),
                HttpMethod = source.HttpMethod,
                NextPartsize = source.NextPartsize,
                ParallelParts = source.ParallelParts,
                RetryParts = source.RetryParts,
                Parameters = Values.Json(source.Parameters),
                PartNumber = source.PartNumber,
                Partsize = source.Partsize,
                Path = source.Path,
                Ref = source.Ref,
                UploadUri = source.UploadUri,
                DirectConnectionInfo = DirectConnectionInfoBridge.Project(source.DirectConnectionInfo, connection, options),
            };
        }
    }
}

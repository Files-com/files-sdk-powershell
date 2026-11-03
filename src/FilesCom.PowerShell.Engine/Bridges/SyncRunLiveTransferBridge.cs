using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's SyncRunLiveTransfer into the module's.
    internal static class SyncRunLiveTransferBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SyncRunLiveTransfer Project(Sdk.SyncRunLiveTransfer source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SyncRunLiveTransfer
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Path = source.Path,
                Status = source.Status,
                BytesCopied = source.BytesCopied,
                BytesTotal = source.BytesTotal,
                Percentage = source.Percentage,
                Eta = source.Eta,
                StartedAt = source.StartedAt,
            };
        }
    }
}

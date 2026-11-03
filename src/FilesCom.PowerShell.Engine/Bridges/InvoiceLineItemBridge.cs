using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's InvoiceLineItem into the module's.
    internal static class InvoiceLineItemBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.InvoiceLineItem Project(Sdk.InvoiceLineItem source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.InvoiceLineItem
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Amount = source.Amount,
                CreatedAt = Values.Utc(source.CreatedAt),
                Description = source.Description,
                Type = source.Type,
                ServiceEndAt = Values.Utc(source.ServiceEndAt),
                ServiceStartAt = Values.Utc(source.ServiceStartAt),
                Plan = source.Plan,
                Site = source.Site,
                PrepaidBytes = source.PrepaidBytes,
                PrepaidBytesExpireAt = Values.Utc(source.PrepaidBytesExpireAt),
                PrepaidBytesUsed = source.PrepaidBytesUsed,
                PrepaidBytesAvailable = source.PrepaidBytesAvailable,
            };
        }
    }
}

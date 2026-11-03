using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's PaymentLineItem into the module's.
    internal static class PaymentLineItemBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PaymentLineItem Project(Sdk.PaymentLineItem source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PaymentLineItem
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Amount = source.Amount,
                CreatedAt = Values.Utc(source.CreatedAt),
                InvoiceId = source.InvoiceId,
                PaymentId = source.PaymentId,
            };
        }
    }
}

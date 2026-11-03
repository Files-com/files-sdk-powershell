using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's AccountLineItem into the module's.
    internal static class AccountLineItemBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AccountLineItem Project(Sdk.AccountLineItem source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AccountLineItem
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Amount = source.Amount,
                Balance = source.Balance,
                CreatedAt = Values.Utc(source.CreatedAt),
                Currency = source.Currency,
                DownloadUri = source.DownloadUri,
                InvoiceLineItems = Values.Array(source.InvoiceLineItems, item => InvoiceLineItemBridge.Project(item, connection, options)),
                Method = source.Method,
                PaymentLineItems = Values.Array(source.PaymentLineItems, item => PaymentLineItemBridge.Project(item, connection, options)),
                PaymentReversedAt = Values.Utc(source.PaymentReversedAt),
                PaymentType = source.PaymentType,
                SiteName = source.SiteName,
                Type = source.Type,
            };
        }
    }
}

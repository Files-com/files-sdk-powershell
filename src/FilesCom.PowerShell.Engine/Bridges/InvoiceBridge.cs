using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Invoice operations with the SDK.
    internal static class InvoiceBridge
    {
        internal static FilesPageSource<Models.AccountLineItem> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Invoices.List(parameters, requestOptions), source => AccountLineItemBridge.Project(source, connection, options));
        }

        internal static Task<Models.AccountLineItem> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Invoices.FindAsync(id, parameters, requestOptions, token), source => AccountLineItemBridge.Project(source, connection, options));
        }
    }
}

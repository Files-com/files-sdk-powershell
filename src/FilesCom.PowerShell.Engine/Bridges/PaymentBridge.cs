using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Payment operations with the SDK.
    internal static class PaymentBridge
    {
        internal static FilesPageSource<Models.AccountLineItem> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Payments.List(parameters, requestOptions), source => AccountLineItemBridge.Project(source, connection, options));
        }

        internal static Task<Models.AccountLineItem> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Payments.FindAsync(id, parameters, requestOptions, token), source => AccountLineItemBridge.Project(source, connection, options));
        }
    }
}

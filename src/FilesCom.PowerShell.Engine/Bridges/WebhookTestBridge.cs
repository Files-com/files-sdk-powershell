using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs WebhookTest operations with the SDK and projects the SDK's WebhookTest into the module's.
    internal static class WebhookTestBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.WebhookTest Project(Sdk.WebhookTest source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.WebhookTest
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Code = source.Code,
                Message = source.Message,
                Status = source.Status,
                Data = AutoBridge.Project(source.Data, connection, options),
                Success = source.Success,
            };
        }

        internal static Task<Models.WebhookTest> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.WebhookTests.CreateAsync(parameters, requestOptions, token), source => WebhookTestBridge.Project(source, connection, options));
        }
    }
}

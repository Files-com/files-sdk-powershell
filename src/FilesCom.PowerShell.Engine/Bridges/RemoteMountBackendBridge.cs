using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs RemoteMountBackend operations with the SDK and projects the SDK's RemoteMountBackend into the module's.
    internal static class RemoteMountBackendBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.RemoteMountBackend Project(Sdk.RemoteMountBackend source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.RemoteMountBackend
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                CanaryFilePath = source.CanaryFilePath,
                Enabled = source.Enabled,
                Fall = source.Fall,
                HealthCheckEnabled = source.HealthCheckEnabled,
                HealthCheckResults = Values.JsonArray(source.HealthCheckResults),
                HealthCheckType = source.HealthCheckType,
                Id = source.Id,
                Interval = source.Interval,
                MinFreeCpu = source.MinFreeCpu,
                MinFreeMem = source.MinFreeMem,
                Priority = source.Priority,
                RemotePath = source.RemotePath,
                RemoteServerId = source.RemoteServerId,
                RemoteServerMountId = source.RemoteServerMountId,
                Rise = source.Rise,
                Status = source.Status,
                UndergoingMaintenance = source.UndergoingMaintenance,
            };
        }

        internal static FilesPageSource<Models.RemoteMountBackend> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.RemoteMountBackends.List(parameters, requestOptions), source => RemoteMountBackendBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteMountBackend> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteMountBackends.FindAsync(id, parameters, requestOptions, token), source => RemoteMountBackendBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteMountBackend> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteMountBackends.CreateAsync(parameters, requestOptions, token), source => RemoteMountBackendBridge.Project(source, connection, options));
        }

        internal static Task ResetStatusAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteMountBackend model ? model.ResetStatusAsync(parameters, token) : client.RemoteMountBackends.ResetStatusAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.RemoteMountBackend> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteMountBackend model ? model.UpdateAsync(parameters, token) : client.RemoteMountBackends.UpdateAsync(id, parameters, requestOptions, token), source => RemoteMountBackendBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteMountBackend model ? model.DeleteAsync(parameters, token) : client.RemoteMountBackends.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}

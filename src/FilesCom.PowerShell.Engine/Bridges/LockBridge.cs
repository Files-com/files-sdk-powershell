using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Lock operations with the SDK and projects the SDK's Lock into the module's.
    internal static class LockBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Lock Project(Sdk.Lock source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Lock
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Path = source.Path,
                Timeout = source.Timeout,
                Depth = source.Depth,
                Recursive = source.Recursive,
                Owner = source.Owner,
                Scope = source.Scope,
                Exclusive = source.Exclusive,
                Token = source.Token,
                Type = source.Type,
                AllowAccessByAnyUser = source.AllowAccessByAnyUser,
                UserId = source.UserId,
                Username = source.Username,
            };
        }

        internal static FilesPageSource<Models.Lock> ListFor(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Locks.ListFor(path, parameters, requestOptions), source => LockBridge.Project(source, connection, options));
        }

        internal static Task<Models.Lock> CreateAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Locks.CreateAsync(path, parameters, requestOptions, token), source => LockBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Lock model ? model.DeleteAsync(parameters, token) : client.Locks.DeleteAsync(path, parameters, requestOptions, token));
        }
    }
}

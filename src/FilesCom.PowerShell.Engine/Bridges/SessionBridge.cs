using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Session operations with the SDK and projects the SDK's Session into the module's.
    internal static class SessionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Session Project(Sdk.Session source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Session
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = Values.Secret(source.Id),
                Language = source.Language,
                ReadOnly = source.ReadOnly,
                SftpInsecureCiphers = source.SftpInsecureCiphers,
            };
        }

        internal static Task<Models.Session> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Sessions.CreateAsync(parameters, requestOptions, token), source => SessionBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Sessions.DeleteAsync(parameters, requestOptions, token));
        }
    }
}

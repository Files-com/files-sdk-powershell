using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Restore operations with the SDK and projects the SDK's Restore into the module's.
    internal static class RestoreBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Restore Project(Sdk.Restore source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Restore
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                EarliestDate = Values.Utc(source.EarliestDate),
                Id = source.Id,
                DirsRestored = source.DirsRestored,
                DirsErrored = source.DirsErrored,
                DirsTotal = source.DirsTotal,
                FilesRestored = source.FilesRestored,
                FilesErrored = source.FilesErrored,
                FilesTotal = source.FilesTotal,
                Prefix = source.Prefix,
                RestorationType = source.RestorationType,
                RestoreInPlace = source.RestoreInPlace,
                RestoreDeletedPermissions = source.RestoreDeletedPermissions,
                UsersRestored = source.UsersRestored,
                UsersErrored = source.UsersErrored,
                UsersTotal = source.UsersTotal,
                ApiKeysRestored = source.ApiKeysRestored,
                PublicKeysRestored = source.PublicKeysRestored,
                TwoFactorAuthenticationMethodsRestored = source.TwoFactorAuthenticationMethodsRestored,
                Status = source.Status,
                UpdateTimestamps = source.UpdateTimestamps,
                WorkspaceId = source.WorkspaceId,
                ErrorMessages = Values.Copy(source.ErrorMessages),
            };
        }

        internal static FilesPageSource<Models.Restore> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Restores.List(parameters, requestOptions), source => RestoreBridge.Project(source, connection, options));
        }

        internal static Task<Models.Restore> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Restores.CreateAsync(parameters, requestOptions, token), source => RestoreBridge.Project(source, connection, options));
        }
    }
}

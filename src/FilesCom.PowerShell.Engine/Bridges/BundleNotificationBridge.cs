using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs BundleNotification operations with the SDK and projects the SDK's BundleNotification into the module's.
    internal static class BundleNotificationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.BundleNotification Project(Sdk.BundleNotification source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.BundleNotification
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                BundleId = source.BundleId,
                Id = source.Id,
                NotifyOnRegistration = source.NotifyOnRegistration,
                NotifyOnUpload = source.NotifyOnUpload,
                NotifyCurrentUser = source.NotifyCurrentUser,
                NotifyUserId = source.NotifyUserId,
                WorkspaceId = source.WorkspaceId,
            };
        }

        internal static FilesPageSource<Models.BundleNotification> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.BundleNotifications.List(parameters, requestOptions), source => BundleNotificationBridge.Project(source, connection, options));
        }

        internal static Task<Models.BundleNotification> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.BundleNotifications.FindAsync(id, parameters, requestOptions, token), source => BundleNotificationBridge.Project(source, connection, options));
        }

        internal static Task<Models.BundleNotification> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.BundleNotifications.CreateAsync(parameters, requestOptions, token), source => BundleNotificationBridge.Project(source, connection, options));
        }

        internal static Task<Models.BundleNotification> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.BundleNotification model ? model.UpdateAsync(parameters, token) : client.BundleNotifications.UpdateAsync(id, parameters, requestOptions, token), source => BundleNotificationBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.BundleNotification model ? model.DeleteAsync(parameters, token) : client.BundleNotifications.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}

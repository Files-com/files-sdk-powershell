using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Notification operations with the SDK and projects the SDK's Notification into the module's.
    internal static class NotificationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Notification Project(Sdk.Notification source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Notification
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Path = source.Path,
                GroupId = source.GroupId,
                GroupName = source.GroupName,
                GroupIds = Values.Copy(source.GroupIds),
                GroupNames = Values.Copy(source.GroupNames),
                TriggeringGroupIds = Values.Copy(source.TriggeringGroupIds),
                TriggeringUserIds = Values.Copy(source.TriggeringUserIds),
                TriggerByShareRecipients = source.TriggerByShareRecipients,
                NotifyUserActions = source.NotifyUserActions,
                NotifyOnCopy = source.NotifyOnCopy,
                NotifyOnDelete = source.NotifyOnDelete,
                NotifyOnDownload = source.NotifyOnDownload,
                NotifyOnMove = source.NotifyOnMove,
                NotifyOnUpload = source.NotifyOnUpload,
                Recursive = source.Recursive,
                SendInterval = source.SendInterval,
                Subject = source.Subject,
                Message = source.Message,
                TriggeringFilenames = Values.Copy(source.TriggeringFilenames),
                WorkspaceId = source.WorkspaceId,
                Unsubscribed = source.Unsubscribed,
                UnsubscribedReason = source.UnsubscribedReason,
                UserId = source.UserId,
                Username = source.Username,
                SuppressedEmail = source.SuppressedEmail,
            };
        }

        internal static FilesPageSource<Models.Notification> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Notifications.List(parameters, requestOptions), source => NotificationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Notification> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Notifications.FindAsync(id, parameters, requestOptions, token), source => NotificationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Notification> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Notifications.CreateAsync(parameters, requestOptions, token), source => NotificationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Notification> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Notification model ? model.UpdateAsync(parameters, token) : client.Notifications.UpdateAsync(id, parameters, requestOptions, token), source => NotificationBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Notification model ? model.DeleteAsync(parameters, token) : client.Notifications.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}

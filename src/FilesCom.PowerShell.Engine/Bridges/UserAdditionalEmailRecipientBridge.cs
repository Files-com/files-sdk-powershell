using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UserAdditionalEmailRecipient operations with the SDK and projects the SDK's UserAdditionalEmailRecipient into the module's.
    internal static class UserAdditionalEmailRecipientBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UserAdditionalEmailRecipient Project(Sdk.UserAdditionalEmailRecipient source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UserAdditionalEmailRecipient
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                UserId = source.UserId,
                WorkspaceId = source.WorkspaceId,
                Email = source.Email,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.UserAdditionalEmailRecipient> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UserAdditionalEmailRecipients.List(parameters, requestOptions), source => UserAdditionalEmailRecipientBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserAdditionalEmailRecipient> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.UserAdditionalEmailRecipients.FindAsync(id, parameters, requestOptions, token), source => UserAdditionalEmailRecipientBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserAdditionalEmailRecipient> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.UserAdditionalEmailRecipients.CreateAsync(parameters, requestOptions, token), source => UserAdditionalEmailRecipientBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserAdditionalEmailRecipient> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.UserAdditionalEmailRecipient model ? model.UpdateAsync(parameters, token) : client.UserAdditionalEmailRecipients.UpdateAsync(id, parameters, requestOptions, token), source => UserAdditionalEmailRecipientBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.UserAdditionalEmailRecipient model ? model.DeleteAsync(parameters, token) : client.UserAdditionalEmailRecipients.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}

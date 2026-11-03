using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Bundle operations with the SDK and projects the SDK's Bundle into the module's.
    internal static class BundleBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Bundle Project(Sdk.Bundle source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Bundle
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Code = source.Code,
                ColorLeft = source.ColorLeft,
                ColorLink = source.ColorLink,
                ColorText = source.ColorText,
                ColorTop = source.ColorTop,
                ColorTopText = source.ColorTopText,
                Url = source.Url,
                Description = source.Description,
                ExpiresAt = Values.Utc(source.ExpiresAt),
                PasswordProtected = source.PasswordProtected,
                Permissions = source.Permissions,
                PreviewOnly = source.PreviewOnly,
                RequireRegistration = source.RequireRegistration,
                RequireShareRecipient = source.RequireShareRecipient,
                RequireLogout = source.RequireLogout,
                ClickwrapBody = source.ClickwrapBody,
                FormFieldSet = FormFieldSetBridge.Project(source.FormFieldSet, connection, options),
                SkipName = source.SkipName,
                SkipEmail = source.SkipEmail,
                StartAccessOnDate = Values.Utc(source.StartAccessOnDate),
                SkipCompany = source.SkipCompany,
                Id = source.Id,
                BypassesSiteExpirationRules = source.BypassesSiteExpirationRules,
                CreatedAt = Values.Utc(source.CreatedAt),
                Deleted = source.Deleted,
                DeletedAt = Values.Utc(source.DeletedAt),
                DontSeparateSubmissionsByFolder = source.DontSeparateSubmissionsByFolder,
                EffectiveExpiresAt = Values.Utc(source.EffectiveExpiresAt),
                MaxUses = source.MaxUses,
                InternalName = source.InternalName,
                Note = source.Note,
                PathTemplate = source.PathTemplate,
                PathTemplateTimeZone = source.PathTemplateTimeZone,
                SendEmailReceiptToUploader = source.SendEmailReceiptToUploader,
                SnapshotId = source.SnapshotId,
                UserId = source.UserId,
                Username = source.Username,
                GroupId = source.GroupId,
                ClickwrapId = source.ClickwrapId,
                InboxId = source.InboxId,
                WatermarkAttachment = ImageBridge.Project(source.WatermarkAttachment, connection, options),
                WatermarkValue = Values.Json(source.WatermarkValue),
                SendOneTimePasswordToRecipientAtRegistration = source.SendOneTimePasswordToRecipientAtRegistration,
                WorkspaceId = source.WorkspaceId,
                HasInbox = source.HasInbox,
                DontAllowFoldersInUploads = source.DontAllowFoldersInUploads,
                RequestedUploadSlots = Values.JsonArray(source.RequestedUploadSlots),
                Paths = Values.Copy(source.Paths),
                Bundlepaths = Values.Array(source.Bundlepaths, item => BundlePathBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.Bundle> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Bundles.List(parameters, requestOptions), source => BundleBridge.Project(source, connection, options));
        }

        internal static Task<Models.Bundle> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Bundles.FindAsync(id, parameters, requestOptions, token), source => BundleBridge.Project(source, connection, options));
        }

        internal static Task<Models.Bundle> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Bundles.CreateAsync(Attachments.ForEngine(parameters), requestOptions, token), source => BundleBridge.Project(source, connection, options));
        }

        internal static Task ShareAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Bundle model ? model.ShareAsync(parameters, token) : client.Bundles.ShareAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.Bundle> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Bundle model ? model.UpdateAsync(Attachments.ForEngine(parameters), token) : client.Bundles.UpdateAsync(id, Attachments.ForEngine(parameters), requestOptions, token), source => BundleBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Bundle model ? model.DeleteAsync(parameters, token) : client.Bundles.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}

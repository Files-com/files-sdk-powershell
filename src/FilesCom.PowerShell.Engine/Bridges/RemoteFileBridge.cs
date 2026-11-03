using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs RemoteFile operations with the SDK and projects the SDK's RemoteFile into the module's.
    internal static class RemoteFileBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.RemoteFile Project(Sdk.RemoteFile source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.RemoteFile
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Path = source.Path,
                CreatedById = source.CreatedById,
                CreatedByApiKeyId = source.CreatedByApiKeyId,
                CreatedByAs2IncomingMessageId = source.CreatedByAs2IncomingMessageId,
                CreatedByAutomationId = source.CreatedByAutomationId,
                CreatedByBundleRegistrationId = source.CreatedByBundleRegistrationId,
                CreatedByInboxId = source.CreatedByInboxId,
                CreatedByRemoteServerId = source.CreatedByRemoteServerId,
                CreatedBySyncId = source.CreatedBySyncId,
                CustomMetadata = Values.Json(source.CustomMetadata),
                DisplayName = source.DisplayName,
                Type = source.Type,
                Size = source.Size,
                CreatedAt = Values.Utc(source.CreatedAt),
                LastModifiedById = source.LastModifiedById,
                LastModifiedByApiKeyId = source.LastModifiedByApiKeyId,
                LastModifiedByAutomationId = source.LastModifiedByAutomationId,
                LastModifiedByBundleRegistrationId = source.LastModifiedByBundleRegistrationId,
                LastModifiedByRemoteServerId = source.LastModifiedByRemoteServerId,
                LastModifiedBySyncId = source.LastModifiedBySyncId,
                Mtime = Values.Utc(source.Mtime),
                ProvidedMtime = Values.Utc(source.ProvidedMtime),
                Crc32 = source.Crc32,
                Md5 = source.Md5,
                Sha1 = source.Sha1,
                Sha256 = source.Sha256,
                MimeType = source.MimeType,
                Region = source.Region,
                Permissions = source.Permissions,
                SubfoldersLocked = source.SubfoldersLocked,
                IsLocked = source.IsLocked,
                DownloadUri = source.DownloadUri,
                DirectConnectionInfo = DirectConnectionInfoBridge.Project(source.DirectConnectionInfo, connection, options),
                PriorityColor = source.PriorityColor,
                PreviewId = source.PreviewId,
                Preview = PreviewBridge.Project(source.Preview, connection, options),
            };
        }

        internal static Task<Models.RemoteFile> DownloadAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.DownloadAsync(parameters, token) : client.RemoteFiles.DownloadAsync(path, parameters, requestOptions, token), source => RemoteFileBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteFile> CreateAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteFiles.CreateAsync(path, parameters, requestOptions, token), source => RemoteFileBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteFile> UpdateAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.UpdateAsync(parameters, token) : client.RemoteFiles.UpdateAsync(path, parameters, requestOptions, token), source => RemoteFileBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.DeleteAsync(parameters, token) : client.RemoteFiles.DeleteAsync(path, parameters, requestOptions, token));
        }

        internal static Task<Models.RemoteFile> FindAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteFiles.FindAsync(path, parameters, requestOptions, token), source => RemoteFileBridge.Project(source, connection, options));
        }

        internal static Task<Models.ZipListEntry[]> ZipListContentsAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.ZipListContentsAsync(parameters, token) : client.RemoteFiles.ZipListContentsAsync(path, parameters, requestOptions, token), source => Values.Array(source, item => ZipListEntryBridge.Project(item, connection, options)));
        }

        internal static Task<Models.FileAction> BeginDeleteAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.BeginDeleteAsync(parameters, token) : client.RemoteFiles.BeginDeleteAsync(path, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> CopyAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.CopyAsync(parameters, token) : client.RemoteFiles.CopyAsync(path, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> CopyToRemoteServerAsync(FilesConnection connection, string path, long destinationId, string destinationPath, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.CopyToRemoteServerAsync(destinationId, destinationPath, parameters, token) : client.RemoteFiles.CopyToRemoteServerAsync(path, destinationId, destinationPath, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> CopyToSnapshotAsync(FilesConnection connection, string path, long destinationId, string destinationPath, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.CopyToSnapshotAsync(destinationId, destinationPath, parameters, token) : client.RemoteFiles.CopyToSnapshotAsync(path, destinationId, destinationPath, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> CopyToChildSiteAsync(FilesConnection connection, string path, long destinationId, string destinationPath, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.CopyToChildSiteAsync(destinationId, destinationPath, parameters, token) : client.RemoteFiles.CopyToChildSiteAsync(path, destinationId, destinationPath, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> MoveAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.MoveAsync(parameters, token) : client.RemoteFiles.MoveAsync(path, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> MoveToRemoteServerAsync(FilesConnection connection, string path, long destinationId, string destinationPath, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.MoveToRemoteServerAsync(destinationId, destinationPath, parameters, token) : client.RemoteFiles.MoveToRemoteServerAsync(path, destinationId, destinationPath, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> MoveToSnapshotAsync(FilesConnection connection, string path, long destinationId, string destinationPath, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.MoveToSnapshotAsync(destinationId, destinationPath, parameters, token) : client.RemoteFiles.MoveToSnapshotAsync(path, destinationId, destinationPath, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> MoveToChildSiteAsync(FilesConnection connection, string path, long destinationId, string destinationPath, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.MoveToChildSiteAsync(destinationId, destinationPath, parameters, token) : client.RemoteFiles.MoveToChildSiteAsync(path, destinationId, destinationPath, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> TransformAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.TransformAsync(parameters, token) : client.RemoteFiles.TransformAsync(path, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> GpgDecryptAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.GpgDecryptAsync(parameters, token) : client.RemoteFiles.GpgDecryptAsync(path, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> GpgEncryptAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.GpgEncryptAsync(parameters, token) : client.RemoteFiles.GpgEncryptAsync(path, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> UnzipAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.UnzipAsync(parameters, token) : client.RemoteFiles.UnzipAsync(path, parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileAction> ZipAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteFiles.ZipAsync(parameters, requestOptions, token), source => FileActionBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileUploadPart[]> BeginUploadAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteFile model ? model.BeginUploadAsync(parameters, token) : client.RemoteFiles.BeginUploadAsync(path, parameters, requestOptions, token), source => Values.Array(source, item => FileUploadPartBridge.Project(item, connection, options)));
        }
    }
}

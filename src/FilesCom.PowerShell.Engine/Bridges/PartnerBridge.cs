using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Partner operations with the SDK and projects the SDK's Partner into the module's.
    internal static class PartnerBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Partner Project(Sdk.Partner source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Partner
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                AllowBypassing2faPolicies = source.AllowBypassing2faPolicies,
                AllowedIps = source.AllowedIps,
                AllowCredentialChanges = source.AllowCredentialChanges,
                AllowProvidingGpgKeys = source.AllowProvidingGpgKeys,
                AllowUserCreation = source.AllowUserCreation,
                CcEmailsToResponsibleParty = source.CcEmailsToResponsibleParty,
                Connections = Values.Array(source.Connections, item => PartnerConnectionBridge.Project(item, connection, options)),
                Id = source.Id,
                AiAssistantPersonalityId = source.AiAssistantPersonalityId,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                Notes = source.Notes,
                PartnerAdminIds = Values.Copy(source.PartnerAdminIds),
                PartnerChannelTemplateId = source.PartnerChannelTemplateId,
                PartnershipRole = source.PartnershipRole,
                ResponsibleGroupId = source.ResponsibleGroupId,
                ResponsibleUserId = source.ResponsibleUserId,
                RootFolder = source.RootFolder,
                ShowPartnerChannelHomePage = source.ShowPartnerChannelHomePage,
                Tags = source.Tags,
                UserIds = Values.Copy(source.UserIds),
            };
        }

        internal static FilesPageSource<Models.Partner> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Partners.List(parameters, requestOptions), source => PartnerBridge.Project(source, connection, options));
        }

        internal static Task<Models.Partner> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Partners.FindAsync(id, parameters, requestOptions, token), source => PartnerBridge.Project(source, connection, options));
        }

        internal static Task<Models.Partner> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Partners.CreateAsync(parameters, requestOptions, token), source => PartnerBridge.Project(source, connection, options));
        }

        internal static Task<Models.Partner> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Partner model ? model.UpdateAsync(parameters, token) : client.Partners.UpdateAsync(id, parameters, requestOptions, token), source => PartnerBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Partner model ? model.DeleteAsync(parameters, token) : client.Partners.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}

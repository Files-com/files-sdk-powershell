using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UserRequest operations with the SDK and projects the SDK's UserRequest into the module's.
    internal static class UserRequestBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UserRequest Project(Sdk.UserRequest source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UserRequest
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                Email = source.Email,
                Details = source.Details,
                Company = source.Company,
            };
        }

        internal static FilesPageSource<Models.UserRequest> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UserRequests.List(parameters, requestOptions), source => UserRequestBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserRequest> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.UserRequests.FindAsync(id, parameters, requestOptions, token), source => UserRequestBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserRequest> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.UserRequests.CreateAsync(parameters, requestOptions, token), source => UserRequestBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.UserRequest model ? model.DeleteAsync(parameters, token) : client.UserRequests.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}

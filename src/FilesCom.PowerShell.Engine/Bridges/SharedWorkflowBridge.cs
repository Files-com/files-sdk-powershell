using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SharedWorkflow operations with the SDK and projects the SDK's SharedWorkflow into the module's.
    internal static class SharedWorkflowBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SharedWorkflow Project(Sdk.SharedWorkflow source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SharedWorkflow
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                SubmissionRevision = source.SubmissionRevision,
                FormFields = Values.Array(source.FormFields, item => FormFieldBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.SharedWorkflow> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SharedWorkflows.List(parameters, requestOptions), source => SharedWorkflowBridge.Project(source, connection, options));
        }

        internal static Task<Models.SharedWorkflow> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SharedWorkflows.FindAsync(id, parameters, requestOptions, token), source => SharedWorkflowBridge.Project(source, connection, options));
        }

        internal static Task<Models.AutomationRun> RunWorkflowAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.SharedWorkflow model ? model.RunWorkflowAsync(parameters, token) : client.SharedWorkflows.RunWorkflowAsync(id, parameters, requestOptions, token), source => AutomationRunBridge.Project(source, connection, options));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace FilesCom.PowerShell.Runtime
{
    // The display settings and aliases of the module's types, added to the importing session from code rather than
    // from .ps1xml files, which execution policy treats as scripts. Each type shows a few properties by default; every
    // property stays available to Select-Object and Format-List *. Properties that .NET naming had to rename, because
    // they are named like their class, also answer to their wire name.
    internal static partial class ModuleTypeData
    {
        internal static void Add()
        {
            Invoke(shell => shell.AddCommand("Microsoft.PowerShell.Utility\\Update-TypeData").AddParameter("TypeData", All().ToArray()).AddParameter("Force").Invoke());
        }

        // Remove-TypeData takes one TypeData at a time, from the pipeline.
        internal static void Remove()
        {
            Invoke(shell => shell.AddCommand("Microsoft.PowerShell.Utility\\Remove-TypeData").Invoke(All().ToArray()));
        }

        private static IEnumerable<TypeData> All()
        {
            yield return Display(typeof(FilesConnection), new[] { "BaseUrl", "AuthenticationMethod", "WorkspaceId", "IsOpen" });
            yield return Display(typeof(FilesUploadResult), new[] { "Path", "Size", "ParallelParts", "Elapsed" });
            yield return Display(typeof(FilesRemoteReadStream), new[] { "Path", "CanRead", "Connection" });
            yield return Display(typeof(FilesRemoteWriteStream), new[] { "Path", "CanWrite", "IsAbandoned", "Connection" });
            yield return Display(typeof(FilesAttachment), new[] { "FileName", "ContentType", "SourceKind", "Length" });
            yield return Display(typeof(FilesAutomaticConcurrency), new[] { "Profile", "StartTarget", "SoftCeiling", "MaxTarget", "Overrides" });
            yield return Display(typeof(FilesAutomaticPartSizing), new[] { "KnownSizeTiers", "MinPartSize", "MaxPartSize", "Overrides" });
            yield return Display(typeof(FilesUploadMetrics), new[] { "ThroughputBytesPerSecond", "Grows", "Shrinks", "Throttles", "ReadyLimitWaits" });
            yield return Display(typeof(FilesUploadAttemptTrace), new[] { "Elapsed", "PartNumber", "Attempt", "StatusCode", "Duration" });
            yield return Display(typeof(FilesUploadDecisionTrace), new[] { "Elapsed", "Decision", "PreviousTarget", "Target" });
            yield return Display(typeof(FilesUploadSizingTrace), new[] { "Elapsed", "Decision", "Size", "Constraint" });
            yield return Display(typeof(FilesUploadTraceSummary), new[] { "Path", "Outcome", "RecordsWritten", "RecordsDropped", "Complete" });
            foreach (TypeData model in Models())
            {
                yield return model;
            }
        }

        // aliases are "Alias=Property" pairs.
        private static TypeData Display(Type type, string[] properties, params string[] aliases)
        {
            var data = new TypeData(type) { DefaultDisplayPropertySet = new PropertySetData(properties) };
            foreach (string alias in aliases)
            {
                string[] names = alias.Split('=');
                data.Members.Add(names[0], new AliasPropertyData(names[0], names[1]));
            }
            return data;
        }

        private static void Invoke(Action<System.Management.Automation.PowerShell> run)
        {
            using (System.Management.Automation.PowerShell shell = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                run(shell);
                if (shell.HadErrors && shell.Streams.Error.Count > 0)
                {
                    throw new InvalidOperationException($"The FilesCom module's type settings could not be changed: {shell.Streams.Error[0]}");
                }
            }
        }
    }
}

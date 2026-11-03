using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Loading
{
    /// <summary>
    /// Imports the module without running any script, so it imports where script files may not run, as under the
    /// Restricted execution policy: refuses an edition the manifest does not list, prepares the engine and adds the
    /// display settings and aliases of the module's types. Removing the module closes the importing runspace's
    /// connections, locally, and removes those settings.
    /// </summary>
    public sealed class ModuleInitializer : IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        private const string ManifestFileName = "FilesCom.PowerShell.psd1";

        /// <summary>Checks the edition, makes the engine loadable and adds the module's type settings.</summary>
        public void OnImport()
        {
            RequireSupportedHost();
            EngineLoader.Register();
            ModuleTypeData.Add();
        }

        /// <summary>
        /// Closes the connections this runspace made, which stops their operations, and removes the module's type
        /// settings. Nothing is sent to Files.com: removing the module does not revoke sessions. Loaded assemblies stay
        /// loaded until the process ends.
        /// </summary>
        public void OnRemove(PSModuleInfo psModuleInfo)
        {
            RunspaceConnections.Current.CloseAll();
            ModuleTypeData.Remove();
        }

        // The manifest's CompatiblePSEditions decides which editions may import this build, and PowerShell 7 needs .NET 8
        // (7.4) or later. The manifest is read as data, never run.
        private static void RequireSupportedHost()
        {
            string edition = EngineLoader.IsDesktop ? "Desktop" : "Core";
            string[] editions = CompatibleEditions(Path.Combine(EngineLoader.ModuleDirectory, ManifestFileName));
            if (Array.IndexOf(editions, edition) < 0)
            {
                throw new NotSupportedException($"This build of the FilesCom module supports PowerShell {string.Join(", ", editions)} editions, not {edition}. On Windows, run it in PowerShell 7.4 or later (pwsh).");
            }
            if (!EngineLoader.IsDesktop && Environment.Version.Major < 8)
            {
                throw new NotSupportedException($"The FilesCom module requires PowerShell 7.4 or later; this PowerShell runs on .NET {Environment.Version}.");
            }
        }

        private static string[] CompatibleEditions(string manifestPath)
        {
            ScriptBlockAst manifest = Parser.ParseFile(manifestPath, out Token[] _, out ParseError[] errors);
            if (errors.Length > 0 || !(manifest.Find(node => node is HashtableAst, searchNestedScriptBlocks: false) is HashtableAst settings))
            {
                throw new InvalidDataException($"The FilesCom module's manifest, {manifestPath}, cannot be read. Reinstall the module.");
            }
            foreach (Tuple<ExpressionAst, StatementAst> setting in settings.KeyValuePairs)
            {
                if (setting.Item1 is StringConstantExpressionAst key && string.Equals(key.Value, "CompatiblePSEditions", StringComparison.OrdinalIgnoreCase))
                {
                    return setting.Item2.FindAll(node => node is StringConstantExpressionAst, searchNestedScriptBlocks: false)
                        .Cast<StringConstantExpressionAst>()
                        .Select(value => value.Value)
                        .ToArray();
                }
            }
            return new[] { "Desktop" };
        }
    }
}

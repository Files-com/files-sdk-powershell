using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace FilesCom.PowerShell.CoreLoader
{
    /// <summary>
    /// Loads the FilesCom engine on PowerShell 7. The engine and its dependencies (the FilesCom SDK,
    /// Microsoft.Extensions, System.Text.Json, Polly.Core and log4net) load into a load context of their own, from the
    /// module's engine folder, so they never conflict with the versions PowerShell or other modules load. The commands
    /// reach the engine through one assembly name: the default context resolves FilesCom.PowerShell.Engine, and nothing
    /// else, from that private context. The contracts assembly loads once, beside the commands, and both sides share it.
    /// </summary>
    public static class EngineContext
    {
        private const string EngineAssemblyName = "FilesCom.PowerShell.Engine";
        private const string ContractsAssemblyName = "FilesCom.PowerShell.Contracts";

        // Framework assemblies whose types the engine's dependencies hand to the .NET libraries' own code, such as the
        // IMeterFactory that Microsoft.Extensions.Http gives SocketsHttpHandler. They must be the host's single copy:
        // a private copy would make those types different from the ones the framework expects.
        private static readonly string[] SharedWithHost = { "System.Diagnostics.DiagnosticSource" };

        private static int registered;
        private static EngineLoadContext context;

        /// <summary>
        /// Makes the engine in <paramref name="engineDirectory"/> the one the commands load. The engine must have the
        /// commands' own version, <paramref name="commandsVersion"/>. Later calls do nothing.
        /// </summary>
        public static void Register(string engineDirectory, Version commandsVersion)
        {
            if (Interlocked.Exchange(ref registered, 1) != 0)
            {
                return;
            }
            string enginePath = Path.Combine(engineDirectory, EngineAssemblyName + ".dll");
            if (!File.Exists(enginePath))
            {
                Volatile.Write(ref registered, 0);
                throw new FileNotFoundException($"The FilesCom module is incomplete: its engine is missing from {engineDirectory}. Reinstall the module.", enginePath);
            }
            Version found = AssemblyName.GetAssemblyName(enginePath).Version;
            if (found != commandsVersion)
            {
                Volatile.Write(ref registered, 0);
                throw new FileLoadException($"The FilesCom module's engine is version {found}, but its commands are version {commandsVersion}. Reinstall the module.", enginePath);
            }
            context = new EngineLoadContext(enginePath);
            AssemblyLoadContext.Default.Resolving += ResolveEngine;
        }

        private static Assembly ResolveEngine(AssemblyLoadContext requestingContext, AssemblyName name)
        {
            return string.Equals(name.Name, EngineAssemblyName, StringComparison.Ordinal) ? context.LoadFromAssemblyName(name) : null;
        }

        private sealed class EngineLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver resolver;

            // Resolves the engine's dependencies from its deps.json, which also picks an operating system's own build
            // of an assembly where the package has one, as for System.Diagnostics.EventLog on Windows.
            internal EngineLoadContext(string enginePath) : base("FilesCom.PowerShell.Engine")
            {
                resolver = new AssemblyDependencyResolver(enginePath);
            }

            // An assembly the engine ships loads here; any other, such as the .NET base libraries or the shared
            // contracts, comes from the default context.
            protected override Assembly Load(AssemblyName name)
            {
                if (string.Equals(name.Name, ContractsAssemblyName, StringComparison.Ordinal))
                {
                    return null;
                }
                if (Array.IndexOf(SharedWithHost, name.Name) >= 0)
                {
                    return Default.LoadFromAssemblyName(new AssemblyName(name.Name));
                }
                string path = resolver.ResolveAssemblyToPath(name);
                if (path == null || !File.Exists(path))
                {
                    return null;
                }
                AssemblyName local = AssemblyName.GetAssemblyName(path);
                if (name.Version != null && local.Version < name.Version)
                {
                    throw new FileLoadException($"The FilesCom module's engine needs {name.Name} {name.Version}, but ships {local.Version}. Reinstall the module.", path);
                }
                return LoadFromAssemblyPath(path);
            }
        }
    }
}

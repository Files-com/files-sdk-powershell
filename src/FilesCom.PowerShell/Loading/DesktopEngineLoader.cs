using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace FilesCom.PowerShell.Loading
{
    // On Windows PowerShell the commands and contracts load from the module's folder and the engine with its dependencies
    // from its desktop folder. .NET Framework finds none of the engine's assemblies there by itself, and where one of them
    // asks for an older version of another than the module ships, as Polly.Core does of Microsoft.Bcl.AsyncInterfaces,
    // it needs a binding decision that an application would make in its configuration file. The module cannot change
    // powershell.exe.config, the GAC or other modules' bindings, so it answers only these requests: made by the module's
    // own code, for an assembly its allowlist names with the same public key token, at a version no newer than the copy
    // it ships, or for the contracts assembly the commands already loaded. Every other request is left to .NET Framework
    // unchanged.
    internal static class DesktopEngineLoader
    {
        private const string AllowlistFileName = "FilesCom.PowerShell.Dependencies.txt";

        private static int registered;
        private static string engineDirectory;
        private static Dictionary<string, AllowedAssembly> allowlist;
        private static Assembly contracts;

        internal static void Register(string directory)
        {
            if (Interlocked.Exchange(ref registered, 1) != 0)
            {
                return;
            }
            engineDirectory = directory;
            allowlist = ReadAllowlist(Path.Combine(directory, AllowlistFileName));
            string enginePath = Path.Combine(directory, EngineLoader.EngineAssemblyName + ".dll");
            if (!File.Exists(enginePath))
            {
                Volatile.Write(ref registered, 0);
                throw new FileNotFoundException($"The FilesCom module is incomplete: its engine is missing from {directory}. Reinstall the module.", enginePath);
            }
            Version found = AssemblyName.GetAssemblyName(enginePath).Version;
            if (found != EngineLoader.CommandsVersion)
            {
                Volatile.Write(ref registered, 0);
                throw new FileLoadException($"The FilesCom module's engine is version {found}, but its commands are version {EngineLoader.CommandsVersion}. Reinstall the module.", enginePath);
            }
            contracts = typeof(FilesObject).Assembly;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveOwnedRequest;
        }

        // Each line names an assembly the desktop folder holds: file name, assembly name, version and public key token.
        private static Dictionary<string, AllowedAssembly> ReadAllowlist(string path)
        {
            if (!File.Exists(path))
            {
                Volatile.Write(ref registered, 0);
                throw new FileNotFoundException($"The FilesCom module is incomplete: {AllowlistFileName} is missing. Reinstall the module.", path);
            }
            var entries = new Dictionary<string, AllowedAssembly>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in File.ReadAllLines(path))
            {
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }
                string[] fields = line.Split('\t');
                entries[fields[1]] = new AllowedAssembly(Path.Combine(engineDirectory, fields[0]), fields[1], new Version(fields[2]), fields[3]);
            }
            return entries;
        }

        private static Assembly ResolveOwnedRequest(object sender, ResolveEventArgs args)
        {
            var requested = new AssemblyName(args.Name);
            if (string.Equals(requested.Name, contracts.GetName().Name, StringComparison.Ordinal))
            {
                return requested.Version == null || requested.Version <= contracts.GetName().Version ? contracts : null;
            }
            if (!allowlist.TryGetValue(requested.Name, out AllowedAssembly allowed) || !allowed.Satisfies(requested) || !IsOwnedRequest(args.RequestingAssembly))
            {
                return null;
            }
            return allowed.Load();
        }

        // Whether the module's own code made the request: the assembly .NET Framework says made it is in the module's
        // folder or its desktop folder or, when it does not say, as while it compiles a method of the SDK, code from
        // those folders is running on this thread. This handler's own frames do not count.
        private static bool IsOwnedRequest(Assembly requesting)
        {
            if (requesting != null)
            {
                return IsModuleAssembly(requesting);
            }
            foreach (StackFrame frame in new StackTrace(false).GetFrames() ?? new StackFrame[0])
            {
                Type type = frame.GetMethod()?.DeclaringType;
                if (type != null && type != typeof(DesktopEngineLoader) && type.DeclaringType != typeof(DesktopEngineLoader) && IsModuleAssembly(type.Assembly))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsModuleAssembly(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                return false;
            }
            string directory = Path.GetDirectoryName(assembly.Location);
            return string.Equals(directory, engineDirectory, StringComparison.OrdinalIgnoreCase)
                || string.Equals(directory, EngineLoader.ModuleDirectory, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class AllowedAssembly
        {
            private readonly string path;
            private readonly string publicKeyToken;
            private readonly Version version;
            private Assembly loaded;

            internal AllowedAssembly(string path, string name, Version version, string publicKeyToken)
            {
                this.path = path;
                Name = name;
                this.version = version;
                this.publicKeyToken = publicKeyToken;
            }

            internal string Name { get; }

            internal bool Satisfies(AssemblyName requested)
            {
                return (requested.Version == null || requested.Version <= version)
                    && string.Equals(Token(requested), publicKeyToken, StringComparison.OrdinalIgnoreCase);
            }

            // Loads the shipped copy after checking it is the assembly the allowlist names.
            internal Assembly Load()
            {
                Assembly assembly = Volatile.Read(ref loaded);
                if (assembly != null)
                {
                    return assembly;
                }
                AssemblyName local = AssemblyName.GetAssemblyName(path);
                if (local.Version != version || !string.Equals(Token(local), publicKeyToken, StringComparison.OrdinalIgnoreCase))
                {
                    throw new FileLoadException($"The FilesCom module's copy of {Name} is not the version it was built with. Reinstall the module.", path);
                }
                assembly = Assembly.LoadFrom(path);
                Volatile.Write(ref loaded, assembly);
                return assembly;
            }

            private static string Token(AssemblyName name)
            {
                byte[] token = name.GetPublicKeyToken();
                return token == null || token.Length == 0 ? "null" : BitConverter.ToString(token).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}

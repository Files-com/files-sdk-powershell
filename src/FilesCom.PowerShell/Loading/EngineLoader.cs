using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace FilesCom.PowerShell.Loading
{
    // Makes the engine loadable for the edition running the module. The commands are one .NET Standard assembly for both
    // editions; the engine is built per runtime: on PowerShell 7 it loads into a private load context from core/engine-*,
    // on Windows PowerShell from the desktop folder through a narrow allowlist.
    internal static class EngineLoader
    {
        internal const string EngineAssemblyName = "FilesCom.PowerShell.Engine";

        internal static bool IsDesktop { get; } = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal);

        internal static string ModuleDirectory { get; } = Path.GetDirectoryName(typeof(EngineLoader).Assembly.Location);

        internal static Version CommandsVersion { get; } = typeof(EngineLoader).Assembly.GetName().Version;

        internal static void Register()
        {
            if (IsDesktop)
            {
                DesktopEngineLoader.Register(Path.Combine(ModuleDirectory, "desktop"));
                return;
            }
            RegisterCore();
        }

        // .NET 10 hosts (PowerShell 7.6) run the engine built for .NET 10; .NET 8 and 9 hosts (7.4, 7.5) the .NET 8 build.
        // The load context itself is made by FilesCom.PowerShell.CoreLoader, which targets .NET 8.
        private static void RegisterCore()
        {
            string core = Path.Combine(ModuleDirectory, "core");
            string engineDirectory = Path.Combine(core, Environment.Version.Major >= 10 ? "engine-net10.0" : "engine-net8.0");
            string loaderPath = Path.Combine(core, "FilesCom.PowerShell.CoreLoader.dll");
            if (!File.Exists(loaderPath))
            {
                throw new FileNotFoundException($"The FilesCom module is incomplete: {loaderPath} is missing. Reinstall the module.", loaderPath);
            }
            Type context = Assembly.LoadFrom(loaderPath).GetType("FilesCom.PowerShell.CoreLoader.EngineContext", throwOnError: true);
            try
            {
                context.GetMethod("Register").Invoke(null, new object[] { engineDirectory, CommandsVersion });
            }
            catch (TargetInvocationException failure) when (failure.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(failure.InnerException).Throw();
            }
        }
    }
}

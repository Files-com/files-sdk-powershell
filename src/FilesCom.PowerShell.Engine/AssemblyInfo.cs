using System.Runtime.CompilerServices;

// Only the module's commands call the engine; its whole surface is internal.
[assembly: InternalsVisibleTo("FilesCom.PowerShell")]

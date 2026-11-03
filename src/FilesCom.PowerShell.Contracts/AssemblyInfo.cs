using System.Runtime.CompilerServices;

// The commands and the engine share the contracts' internal members: connection lifetimes, page sources and the
// setters of model properties, which only the engine fills.
[assembly: InternalsVisibleTo("FilesCom.PowerShell")]
[assembly: InternalsVisibleTo("FilesCom.PowerShell.Engine")]

namespace FilesCom.PowerShell
{
    /// <summary>
    /// The least severe engine diagnostic a connection's commands produce (<c>Connect-FilesAccount -LogLevel</c>).
    /// </summary>
    public enum FilesLogLevel
    {
        /// <summary>
        /// Everything, including the engine's DEBUG diagnostics, which reach the Debug stream only for a connection made
        /// with -DiagnosticDebug.
        /// </summary>
        Debug,

        /// <summary>Requests, retries and other ordinary diagnostics, on the Verbose stream, and everything more severe.</summary>
        Information,

        /// <summary>Warnings and errors only.</summary>
        Warning,

        /// <summary>Errors only.</summary>
        Error,

        /// <summary>No engine diagnostics.</summary>
        None,
    }
}

using System;
using Microsoft.Extensions.Logging;

namespace FilesCom.PowerShell.Engine
{
    // The logger factory every client the module makes logs through (FilesConfiguration.LoggerFactory), so the engine
    // never uses log4net for the module's clients and nothing global is configured: another application's or module's
    // loggers in the same process keep their own settings. Each message goes to the sink of the command operation whose
    // asynchronous flow logged it; a message logged outside any command operation goes nowhere. Messages at Information
    // and above, which reach the Verbose stream of any command, lose the credentials, query and fragment of any URL in
    // them. Debug and Trace messages are the engine's explicit full diagnostics and reach only a sink that asked for them,
    // from a connection made with -DiagnosticDebug; they pass unchanged. A connection's -LogLevel is the client's
    // FilesConfiguration.MinimumLogLevel, which the engine applies before a message reaches this factory.
    internal sealed class EngineLogging : ILoggerFactory
    {
        internal static readonly EngineLogging Factory = new EngineLogging();

        private static readonly SinkLogger Logger = new SinkLogger();

        private EngineLogging()
        {
        }

        internal static LogLevel? MinimumLevel(FilesLogLevel? level)
        {
            switch (level)
            {
                case FilesLogLevel.Debug:
                    return LogLevel.Debug;
                case FilesLogLevel.Information:
                    return LogLevel.Information;
                case FilesLogLevel.Warning:
                    return LogLevel.Warning;
                case FilesLogLevel.Error:
                    return LogLevel.Error;
                case FilesLogLevel.None:
                    return LogLevel.None;
                default:
                    return null;
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return Logger;
        }

        // The factory routes by operation, so providers have nowhere to add to.
        public void AddProvider(ILoggerProvider provider)
        {
        }

        // Shared by every client, which never disposes a factory it was given.
        public void Dispose()
        {
        }

        private sealed class SinkLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
                where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                FilesLogSink sink = FilesLogSink.Current;
                return sink != null && logLevel != LogLevel.None && (logLevel >= LogLevel.Information || sink.Debug);
            }

            // Called on the thread that logged, which is running the operation's flow. It returns at once and never
            // throws, so it cannot change what the engine does.
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }
                try
                {
                    bool debug = logLevel < LogLevel.Information;
                    string message = formatter != null ? formatter(state, exception) : state?.ToString();
                    string text = debug
                        ? message + (exception != null ? Environment.NewLine + exception : "")
                        : SafeText.WithoutUrlCredentials(message);
                    FilesLogSink.Current?.Add(debug, "FilesCom: " + text);
                }
                catch (Exception)
                {
                    // A message that cannot be rendered is left out; the engine's work goes on.
                }
            }
        }
    }
}

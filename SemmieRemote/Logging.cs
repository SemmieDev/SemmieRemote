namespace SemmieRemote;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

public static class Logging {
    private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder
        .ClearProviders()
        .AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>()
        .AddConsole(options => {
            options.FormatterName = "custom";
        })
        .SetMinimumLevel(LogLevel.Information)
    );

    public static ILogger GetLogger(string name) {
        return LoggerFactory.CreateLogger(name);
    }

    public static ILogger<T> GetLogger<T>() {
        return LoggerFactory.CreateLogger<T>();
    }

    internal class CustomConsoleFormatter() : ConsoleFormatter("custom") {
        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter) {
            textWriter.WriteLine($"[{logEntry.Category}] [{logEntry.LogLevel}]: {logEntry.State}");

            if (logEntry.Exception == null) return;

            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }
}
using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace Infrastructure.Logging
{
    public static class SerilogConfigurator
    {
        /// <summary>
        /// Configure and assign the global Serilog logger.
        /// Returns the configured ILogger instance.
        /// </summary>
        public static ILogger Configure(string? logFile, bool verbose)
        {
            var min = verbose ? LogEventLevel.Debug : LogEventLevel.Information;
            return Configure(logFile, min);
        }

        /// <summary>
        /// Configure Serilog with an explicit minimum level.
        /// </summary>
        public static ILogger Configure(string? logFile, LogEventLevel minimumLevel)
        {
            var cfg = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(logFile))
            {
                try
                {
                    var dir = Path.GetDirectoryName(logFile) ?? AppContext.BaseDirectory;
                    if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);

                    // Use a single exact file name (don't append date) so callers/tests that expect a fixed
                    // filename can find it. Use shared=true to allow multiple processes to read it.
                    cfg = cfg.WriteTo.File(logFile, rollingInterval: RollingInterval.Infinite, shared: true);
                }
                catch (Exception)
                {
                    // If file sink cannot be created, continue with console-only logger.
                }
            }

            Log.Logger = cfg.CreateLogger();
            return Log.Logger;
        }

        /// <summary>
        /// Configure Serilog using environment variables.
        /// Recognized variables:
        /// - LOG_FILE: path to log file
        /// - LOG_LEVEL: optional log level name (Debug, Information, Warning, Error)
        /// - LOG_VERBOSE: if present and set to true, enables verbose (Debug) level
        /// </summary>
        public static ILogger ConfigureFromEnvironment()
        {
            var logFile = Environment.GetEnvironmentVariable("LOG_FILE");
            var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
            var logVerbose = Environment.GetEnvironmentVariable("LOG_VERBOSE");

            // Determine minimum log level: LOG_LEVEL has priority, then LOG_VERBOSE fallback.
            LogEventLevel minimum = LogEventLevel.Information;
            if (!string.IsNullOrWhiteSpace(logLevel))
            {
                if (Enum.TryParse<LogEventLevel>(logLevel, true, out var parsed))
                {
                    minimum = parsed;
                }
            }
            else if (!string.IsNullOrWhiteSpace(logVerbose) && logVerbose.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                minimum = LogEventLevel.Debug;
            }

            return Configure(logFile, minimum);
        }
    }
}

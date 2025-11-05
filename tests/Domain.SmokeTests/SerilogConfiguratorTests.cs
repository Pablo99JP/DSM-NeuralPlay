using System;
using System.IO;
using Serilog;
using Infrastructure.Logging;
using Xunit;

namespace Domain.SmokeTests
{
    [Collection("NonParallel")]
    public class SerilogConfiguratorTests
    {
        [Fact]
        public void ConfigureFromEnvironment_WritesToLogFile_WhenEnvSet()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "serilog_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);
            var logPath = Path.Combine(tmp, "init.log");

            try
            {
                Environment.SetEnvironmentVariable("LOG_FILE", logPath);
                Environment.SetEnvironmentVariable("LOG_VERBOSE", "true");

                var logger = SerilogConfigurator.ConfigureFromEnvironment();
                // Use the returned logger instance directly to avoid races with global/static logger state
                logger.Information("Serilog configurator test: hello world");
                Log.CloseAndFlush();

                // Wait for the sink to create the file (give it a generous but bounded timeout)
                // Increased timeout to be tolerant with async sinks on CI or slow FS.
                for (int i = 0; i < 100 && !File.Exists(logPath); i++)
                {
                    System.Threading.Thread.Sleep(100);
                }
                Assert.True(File.Exists(logPath), "Expected log file to be created by Serilog configurator");

                // Read the file allowing shared read (Serilog may keep the file open with shared access).
                // Retry for a short time until the logged text appears to avoid flakes from async writes.
                string content = string.Empty;
                var found = false;
                // Retry for a longer period to avoid flakes from async writes/FS delays.
                for (int attempt = 0; attempt < 400 && !found; attempt++)
                {
                    try
                    {
                        using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var sr = new StreamReader(fs))
                        {
                            content = sr.ReadToEnd();
                        }
                        if (!string.IsNullOrWhiteSpace(content) && content.IndexOf("hello world", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            found = true;
                            break;
                        }
                    }
                    catch (IOException)
                    {
                        // transient I/O; ignore and retry
                    }
                    System.Threading.Thread.Sleep(50);
                }

                Assert.True(found, $"Expected log file to contain 'hello world' but content was: '{content}'");
            }
            finally
            {
                // cleanup
                try { Environment.SetEnvironmentVariable("LOG_FILE", null); } catch { }
                try { Environment.SetEnvironmentVariable("LOG_VERBOSE", null); } catch { }
                try { Directory.Delete(tmp, recursive: true); } catch { }
            }
        }
    }
}

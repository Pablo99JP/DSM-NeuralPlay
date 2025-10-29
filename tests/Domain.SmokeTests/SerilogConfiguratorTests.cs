using System;
using System.IO;
using Serilog;
using Infrastructure.Logging;
using Xunit;

namespace Domain.SmokeTests
{
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
                Log.Information("Serilog configurator test: hello world");
                Log.CloseAndFlush();

                // Wait briefly for the sink to create the file (it should be immediate, but be tolerant)
                for (int i = 0; i < 5 && !File.Exists(logPath); i++)
                {
                    System.Threading.Thread.Sleep(100);
                }
                Assert.True(File.Exists(logPath), "Expected log file to be created by Serilog configurator");

                // Read the file allowing shared read (Serilog may keep the file open with shared access)
                string content;
                using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    content = sr.ReadToEnd();
                }
                Assert.Contains("hello world", content, StringComparison.OrdinalIgnoreCase);
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

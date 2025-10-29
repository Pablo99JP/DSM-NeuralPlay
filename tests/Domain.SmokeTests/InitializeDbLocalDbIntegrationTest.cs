using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Domain.SmokeTests
{
    public class InitializeDbLocalDbIntegrationTest
    {
        [Fact]
        public async Task InitializeDb_UsesLocalDb_WhenLocalDbAvailable()
        {
            // This integration test runs only when CI sets LOCALDB_AVAILABLE=true.
            var localDbFlag = Environment.GetEnvironmentVariable("LOCALDB_AVAILABLE");
            if (string.IsNullOrWhiteSpace(localDbFlag) || !localDbFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                // Skip locally (no LocalDB available on developer machines by default)
                return;
            }

            var tmp = Path.Combine(Path.GetTempPath(), "initdb_localdb_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);
            var logPath = Path.Combine(tmp, "init.log");
            var dbName = "ci_integration_" + Guid.NewGuid().ToString("N");

            try
            {
                var args = new[] { "--mode=schemaexport", $"--db-name={dbName}", $"--data-dir={tmp}", $"--log-file={logPath}" };
                using var sw = new StringWriter();
                var exit = await InitializeDbService.RunAsync(args, sw);
                Assert.Equal(0, exit);
                var output = sw.ToString();
                Assert.False(string.IsNullOrWhiteSpace(output));

                // Verify that the initializer attempted LocalDB export
                Assert.Contains("Attempting SchemaExport to LocalDB", output);

                // If LocalDB was used, MDF should be referenced in output or created in data dir
                var mdfPath = Path.Combine(tmp, dbName + ".mdf");
                // It's possible creation is done under a different account; assert that output mentions LocalDB success or mdf path
                Assert.True(output.Contains("SchemaExport to LocalDB completed") || File.Exists(mdfPath), "Expected LocalDB schema export to be attempted/completed.");
            }
            finally
            {
                try { Directory.Delete(tmp, recursive: true); } catch { }
            }
        }
    }
}

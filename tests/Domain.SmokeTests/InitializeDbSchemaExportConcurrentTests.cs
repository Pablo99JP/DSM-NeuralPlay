using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class InitializeDbSchemaExportConcurrentTests
{
    [Fact]
    public async Task Concurrent_SchemaExport_With_Seed_Completes()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "initdb_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var logFile = Path.Combine(tmp, "init.log");
        var args = new[] { "--mode=schemaexport", "--seed", "--db-name=concurrent_test_db", $"--data-dir={tmp}", $"--log-file={logFile}" };

        var tasks = Enumerable.Range(0, 3).Select(_ => InitializeDbService.RunAsync(args, new StringWriter())).ToArray();
        var results = await Task.WhenAll(tasks);
        Assert.All(results, r => Assert.Equal(0, r));

        // Expect at least one artifact (sqlite or mdf/log) to exist under tmp
        Assert.True(Directory.EnumerateFileSystemEntries(tmp).Any(), "Expected artifacts in temp data dir");
    }
}

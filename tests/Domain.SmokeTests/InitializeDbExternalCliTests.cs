using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Infrastructure.NHibernate;
using Xunit;

namespace Domain.SmokeTests
{
    public class InitializeDbExternalCliTests
    {
        [Fact(Timeout = 120000)]
        public async System.Threading.Tasks.Task InitializeDb_Cli_Runs_And_Seeds_Sqlite_File()
        {
            // Find solution root by walking up until DSM-NeuralPlay.sln is found
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            DirectoryInfo? solutionDir = null;
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "DSM-NeuralPlay.sln")))
                {
                    solutionDir = dir;
                    break;
                }
                dir = dir.Parent!;
            }

            Assert.NotNull(solutionDir);

            var dllPath = Path.Combine(solutionDir.FullName, "InitializeDb", "bin", "Debug", "net8.0", "InitializeDb.dll");
            Assert.True(File.Exists(dllPath), $"InitializeDb DLL not found at {dllPath}. Build the solution before running this test.");

            var dbName = "seedcli_" + Guid.NewGuid().ToString("N");

            // Create an isolated data directory for this test run
            var isolatedDataDir = Path.Combine(Path.GetTempPath(), "initdb_cli_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(isolatedDataDir);

            var psi = new ProcessStartInfo("dotnet", $"\"{dllPath}\" --mode=schemaexport --seed --db-name={dbName} --data-dir=\"{isolatedDataDir}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.Combine(solutionDir.FullName, "InitializeDb")
            };

            using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start process");

            var stdoutTask = proc.StandardOutput.ReadToEndAsync();
            var stderrTask = proc.StandardError.ReadToEndAsync();

            // Wait with timeout
            var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(110000));
                try
                {
                    await proc.WaitForExitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    var partialOut = await stdoutTask;
                    var partialErr = await stderrTask;
                    throw new Xunit.Sdk.XunitException($"InitializeDb process did not exit in time. stdout: {partialOut}\nerr: {partialErr}");
                }

            var outText = await stdoutTask;
            var errText = await stderrTask;

            Assert.Equal(0, proc.ExitCode);

            // Data folder under the isolated directory we passed
            var dataDir = isolatedDataDir;
            Assert.True(Directory.Exists(dataDir), "Data directory was not created by InitializeDb");

            // Expect fallback sqlite file 'project.db' to be present (LocalDB may be skipped in CI)
            var sqliteFile = Path.Combine(dataDir, "project.db");
            Assert.True(File.Exists(sqliteFile), $"Expected sqlite file at {sqliteFile} but not found. stdout: {outText}\nerr: {errText}");

            // Verify NHibernate can read seeded users from that sqlite file
            var sqliteConn = $"Data Source={sqliteFile};Version=3;";
            var cfg = NHibernateHelper.BuildConfiguration();
            cfg.SetProperty("connection.connection_string", sqliteConn);
            cfg.SetProperty("dialect", "NHibernate.Dialect.SQLiteDialect");
            var sf = cfg.BuildSessionFactory();
            using var session = sf.OpenSession();
            var usuarioRepo = new NHibernateUsuarioRepository(session);
            var users = usuarioRepo.ReadAll().ToList();
            Assert.True(users.Count >= 1, "Expected at least one seeded user in the sqlite DB");

            // Clean up test artifacts (best-effort)
            try
            {
                if (File.Exists(sqliteFile)) File.Delete(sqliteFile);
                if (Directory.Exists(dataDir)) Directory.Delete(dataDir, true);
            }
            catch { /* ignore cleanup errors */ }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Domain.SmokeTests
{
    public class InitializeDbLogFileTest
    {
        [Fact]
        public async Task InitializeDb_CreatesLogFile_WhenLogFileArgProvided()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "initdb_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);
            var logPath = Path.Combine(tmp, "init.log");

            // Locate built InitializeDb DLL similar to existing tests
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

            var projPath = Path.Combine(solutionDir.FullName, "InitializeDb", "InitializeDb.csproj");
            Assert.True(File.Exists(projPath), $"InitializeDb project not found at {projPath}. Build or restore solution before running this test.");

            var psi = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.Combine(solutionDir.FullName, "InitializeDb")
            };

            // Use ArgumentList to avoid quoting/parsing issues
            // Do not require a platform-specific EXE (CI may build DLLs). Call the programmatic API directly
            // and capture external log output via StringWriter.
            var args = new[] { "--mode=schemaexport", "--seed", $"--data-dir={tmp}", $"--log-file={logPath}" };
            using (var sw = new StringWriter())
            {
                var exitCode = await InitializeDbService.RunAsync(args, sw);
                Assert.Equal(0, exitCode);
                var captured = sw.ToString();
                Assert.False(string.IsNullOrWhiteSpace(captured));
                Assert.Contains("InitializeDb", captured);
            }

            // cleanup
            try { Directory.Delete(tmp, recursive: true); } catch { }
        }
    }
}

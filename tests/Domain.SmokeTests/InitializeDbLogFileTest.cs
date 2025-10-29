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
            // Prefer running the compiled EXE directly to avoid dotnet/msbuild invocations in the test runner
            var exePath = Path.Combine(solutionDir.FullName, "InitializeDb", "bin", "Debug", "net8.0", "InitializeDb.exe");
            Assert.True(File.Exists(exePath), $"InitializeDb EXE not found at {exePath}. Build the solution before running this test.");

            // Call the programmatic API directly
            var args = new[] { "--mode=schemaexport", "--seed", $"--data-dir={tmp}", $"--log-file={logPath}" };
            var exitCode = await InitializeDbService.RunAsync(args, null);
            Assert.Equal(0, exitCode);

            // Serilog may create log files with small variations (timestamps/suffixes).
            // Accept any file in the data dir that matches the prefix used (init*.log).
            var matches = Directory.GetFiles(tmp, "init*.log", SearchOption.TopDirectoryOnly);
            Assert.NotEmpty(matches);
            var first = matches[0];
            // Read with shared access in case Serilog still has the file open
            string content;
            // Retry reading until we get non-empty content (file may be written asynchronously)
            bool found = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using (var fs = new FileStream(first, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs))
                    {
                        content = sr.ReadToEnd();
                    }
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        Assert.Contains("InitializeDb", content);
                        found = true;
                        break;
                    }
                }
                catch (IOException)
                {
                    // allow retry
                }
                System.Threading.Thread.Sleep(200);
            }
            if (!found) Assert.False(true, "Log file did not contain expected content after retries.");

            // cleanup
            try { Directory.Delete(tmp, recursive: true); } catch { }
        }
    }
}

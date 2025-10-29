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

            var dllPath = Path.Combine(solutionDir.FullName, "InitializeDb", "bin", "Debug", "net8.0", "InitializeDb.dll");
            Assert.True(File.Exists(dllPath), $"InitializeDb DLL not found at {dllPath}. Build the solution before running this test.");

            var psi = new ProcessStartInfo("dotnet", $"\"{dllPath}\" --mode=schemaexport --seed --data-dir=\"{tmp}\" --log-file=\"{logPath}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.Combine(solutionDir.FullName, "InitializeDb")
            };

            using var p = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet process");

            var stdoutTask = p.StandardOutput.ReadToEndAsync();
            var stderrTask = p.StandardError.ReadToEndAsync();

            var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(110000));
            try
            {
                await p.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                var partialOut = await stdoutTask;
                var partialErr = await stderrTask;
                throw new Xunit.Sdk.XunitException($"InitializeDb process did not exit in time. stdout: {partialOut}\nerr: {partialErr}");
            }

            var outText = await stdoutTask;
            var errText = await stderrTask;

            Assert.Equal(0, p.ExitCode);
            Assert.True(File.Exists(logPath), $"Expected log file at {logPath} to exist. stdout: {outText}\nstderr: {errText}");

            var content = File.ReadAllText(logPath);
            Assert.Contains("InitializeDb log started", content);

            // cleanup
            try { Directory.Delete(tmp, recursive: true); } catch { }
        }
    }
}

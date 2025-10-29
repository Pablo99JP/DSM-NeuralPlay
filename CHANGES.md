# Changelog

Un resumen de los cambios introducidos en la rama `feature/centralize-logging-t1-t2`.

## 2025-10-29 — Centralize logging, T1+T2
- feat: Centralized Serilog configuration in `Infrastructure/Logging/SerilogConfigurator`.
  - Exposes `Configure(logFile, LogEventLevel)` and `ConfigureFromEnvironment()` which reads `LOG_FILE`, `LOG_LEVEL`, `LOG_VERBOSE`.
- ci: CI workflow (`.github/workflows/ci-matrix.yml`) now exports `LOG_FILE`, `LOG_VERBOSE`, and `LOG_LEVEL` during `InitializeDb` steps so logs are captured in artifacts.
- test: Introduced an in-memory Serilog sink for structured log assertions to avoid fragile file-based assertions. Files:
  - `tests/Domain.SmokeTests/TestSinks/InMemorySerilogSink.cs`
  - `tests/Domain.SmokeTests/SerilogInMemoryTests.cs`
  - `tests/Domain.SmokeTests/SerilogConfiguratorTests.cs`
- fix: Removed an explicit StreamWriter workaround previously used to force a fixed filename; tests updated to use the programmatic `InitializeDbService.RunAsync(args, TextWriter)` hook where appropriate.

## Rationale
- Improves observability and test reliability by centralizing logging configuration and enabling structured log assertions.
- Allows CI to control verbosity and file path through env vars without changing program args.

## How to test locally
1. Build and run tests:

```powershell
cd <repo-root>
dotnet restore
dotnet build
dotnet test
```

2. Run the initializer with explicit log file and observe logs:

```powershell
dotnet run --project InitializeDb/InitializeDb.csproj -- --mode=schemaexport --seed --data-dir=./tmp --log-file=./tmp/init.log
```

3. To change verbosity via environment variables:

```powershell
$env:LOG_LEVEL = 'Debug'
$env:LOG_FILE = 'InitializeDb/Data_CI/init.log'
dotnet run --project InitializeDb/InitializeDb.csproj -- --mode=schemaexport --seed --data-dir=InitializeDb/Data_CI --log-file=InitializeDb/Data_CI/init.log
```

## Notes
- All tests passed locally when this changelog was created. CI will run SCA tasks and upload reports; please check artifacts on the PR once created.

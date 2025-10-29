# PR draft: InitializeDb seed + DI + tests + CI matrix

This PR contains the following changes:

- Implement `--seed` idempotent seeding in `InitializeDb/Program.cs` using CENs and NHibernate repositories.
- Add `--data-dir` parameter to `InitializeDb` to isolate DB files for tests/CI.
- Implement minimal DI registration for the initializer using `Microsoft.Extensions.DependencyInjection` (register SessionFactory, ISession, repositories, UnitOfWork, CENs).
- Add integration tests:
  - `InitializeDbSeedIdempotencyTests` verifies seed idempotency against a SQLite file.
  - `InitializeDbExternalCliTests` runs the compiled `InitializeDb.dll` to exercise CLI flows and validate output (now async/await).
- Add GitHub Actions CI matrix (`.github/workflows/ci-matrix.yml`) for ubuntu + windows that runs SchemaExport+seed and `dotnet test`.
- Add `CHANGELOG.md` describing the changes and security notes.

Notes for reviewers:
- The initializer now accepts `--data-dir` which is used by CI and tests to avoid polluting repo-local Data folder.
- DI is intentionally minimal and scoped to the seeding operation; it registers only the repositories and CENs used by the seed. This can be extended to register more services as needed.
- The CI matrix uploads are not included in this PR; I recommend adding artifact uploads for `InitializeDb/Data_CI` so failing jobs preserve DB and logs for debugging.

How to test locally:

1. Build solution:

```powershell
dotnet build
```

2. Run InitializeDb with seed into a temp data dir:

```powershell
dotnet run --project InitializeDb/InitializeDb.csproj -- --mode=schemaexport --seed --data-dir "C:\temp\initdb_test"
```

3. Run tests:

```powershell
dotnet test
```

Suggested next steps:
- Upload CI artifacts (DB files and logs) from workflow to ease troubleshooting.
- Expand DI registrations to support all repositories/CENs/CPs.
- Add verbose logging option to `InitializeDb` and wire structured logs.


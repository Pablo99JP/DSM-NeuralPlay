DSM-NeuralPlay
================

Overview
--------
This repository contains a Clean Architecture / DDD scaffold for the "NeuralPlay" domain. The domain model is available in `domain.model.json` (generated from `dominio.puml`). The core is implemented in `ApplicationCore` (EN, CEN, CP, repository interfaces). There are both in-memory adapters for local validation and NHibernate-based persistence (XML mappings) plus a small `InitializeDb` console that can export schema and optionally seed via the domain layer.

Quick commands (Windows PowerShell)
----------------------------------
# Restore/Build core projects
dotnet build .\ApplicationCore\ApplicationCore.csproj

dotnet build .\InitializeDb\InitializeDb.csproj

# Run the initialization harness (in-memory mode by default)
# This program exercises CEN/CP logic without touching disk or a real DB:
dotnet run --project .\InitializeDb\InitializeDb.csproj

InitializeDb: modos y flags
---------------------------
`InitializeDb` soporta los siguientes modos y flags relevantes:

- Modo por defecto (`inmemory`):
	- Ejecuta la validación en memoria usando los repositorios `InMemory` y los CEN/CP del dominio.

- Schema export (crear esquema con NHibernate):
	- `dotnet run --project .\InitializeDb\InitializeDb.csproj -- --mode=schemaexport`
	- El inicializador intentará crear/adjuntar un archivo LocalDB MDF en `--data-dir` (por defecto `InitializeDb/Data/ProjectDatabase.mdf`). Si LocalDB no está disponible o la operación falla, hará fallback a SQLite creando `project.db` en la misma carpeta.

Flags útiles (CLI):

- `--mode=<inmemory|schemaexport>` (default: `inmemory`)
- `--seed` — Ejecuta un seed idempotente usando CEN/CP (opcional, solo si quieres poblar datos).
- `--db-name=<name>` — nombre de la base (por defecto `ProjectDatabase`).
- `--force-drop` — permite eliminar MDF existente antes de crear uno nuevo (destructivo).
- `--confirm` — requerido junto con `--force-drop` para confirmar la acción destructiva.
- `--data-dir=<path>` — directorio donde se crearán los artifacts (MDF, project.db, logs). Útil en CI.
- `--log-file=<path>` — ruta del archivo de log para Serilog.
- `--verbose` o `-v` — nivel de log más detallado.

Ejemplo (crear esquema y seed en modo schemaexport; confirmando drop):

```powershell
dotnet run --project .\InitializeDb\InitializeDb.csproj -- --mode=schemaexport --db-name=ProjectDatabase --force-drop --confirm --seed --data-dir=InitializeDb/Data --log-file=InitializeDb/Data/init.log
```

# Serilog / Logging
The project uses a centralized Serilog configurator (`Infrastructure/Logging/SerilogConfigurator`). You can control logging via environment variables or command line flags:

- `LOG_FILE` — path to a log file (Serilog file sink will be used if present).
- `LOG_LEVEL` — explicit Serilog level name (e.g. `Debug`, `Information`).
- `LOG_VERBOSE=true` — shorthand to enable `Debug` level.

The CLI `--log-file` is passed to the configurator; tests call the programmatic API `InitializeDbService.RunAsync(...)` and can provide an external `TextWriter` to capture output.

# Tests
Run the test suite:

```powershell
dotnet test .\tests\Domain.SmokeTests\Domain.SmokeTests.csproj
```

Where to look
-------------
- Domain entities and business logic: `ApplicationCore/Domain/EN` and `ApplicationCore/Domain/CEN`.
- Repository interfaces: `ApplicationCore/Domain/Repositories`.
- In-memory adapters (tests/dev): `ApplicationCore/Infrastructure/Memory`.
- NHibernate implementation and mappings: `Infrastructure/NHibernate` (look for `*.hbm.xml` files and `NHibernateHelper`).
- InitializeDb executable and programmatic API: `InitializeDb/InitializeDbService.cs` and `InitializeDb/Program.cs`.
- Tests (smoke/unit): `tests/Domain.SmokeTests`.

# CI, SCA and artifacts
- The GitHub Actions workflow generates SCA artifacts (`.github/ci-reports/vulnerable.json` and `outdated.json`) and uploads them as job artifacts.
- InitializeDb artifacts (logs, MDF/DB file or SQLite DB) are zipped per-run and uploaded by CI; see `.github/workflows/ci-matrix.yml` for details.
- A `SECURITY.md` file documents the SCA/pinning policy and Dependabot cadence.

Notes and caveats
-----------------
- NHibernate-based mappings and repositories are present (XML `.hbm.xml`) and `NHibernateHelper` resolves mappings at runtime. `InitializeDb` performs schema export using NHibernate and falls back to SQLite when LocalDB is not available.
- `--seed` is opt-in: the initializer will only seed data if you pass `--seed` (or call the programmatic API with seeding enabled). The seed is implemented via the domain CENs/CPs and is idempotent.
- Password hashing used in tests/seed is for demo/validation only — replace with a secure hashing algorithm (PBKDF2 / BCrypt / Argon2) before production use.
- Many transitive packages are reported as outdated by SCA; no High/Critical advisories were detected in the most recent scan, but you should monitor CI/Dependabot and apply targeted pins or upgrades when advisories appear.

If you want, I can add more CP unit tests or improve the README further (usage examples, advanced debugging tips, or a short troubleshooting section for LocalDB on Windows). 
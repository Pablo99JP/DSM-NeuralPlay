# CHANGELOG

## [Unreleased] - 2025-10-29

### Added
- Implemented `InitializeDb --seed` idempotent seeding using CENs and NHibernate repositories.
- Added `--data-dir` flag to `InitializeDb` to allow isolation of database files (useful for tests and CI).
- Added integration tests validating idempotency and an external CLI test that runs `InitializeDb` and verifies seeded data.
- Added GitHub Actions CI matrix (`.github/workflows/ci-matrix.yml`) to run SchemaExport+seed and tests on Ubuntu and Windows runners.

### Changed
- `InitializeDb` now falls back to SQLite file when LocalDB is not available; CLI supports safety flags `--force-drop` and `--confirm`.
- NHibernate helper improved to load embedded mappings and export schema programmatically.
 - Replaced `System.Data.SqlClient` with `Microsoft.Data.SqlClient` in projects that target SQL Server (notably `Infrastructure` and `InitializeDb`) to address known vulnerabilities in the older package. Solution builds and full test suite passed locally (38/38).

### Notes
- The replacement uses `Microsoft.Data.SqlClient` v6.1.2 in the edited projects. Consider aligning versions across all projects that connect to SQL Server and running Dependabot/renovate to keep the package up to date.
- If you need a rollback or prefer a different supported version, I can update the CSProj entries accordingly and run the test suite again.

### Security
- Upgraded NHibernate and related dependencies to address advisory notices; ensure Dependabot runs for future updates.


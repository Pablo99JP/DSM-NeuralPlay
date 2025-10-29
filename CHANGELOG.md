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

### Security
- Upgraded NHibernate and related dependencies to address advisory notices; ensure Dependabot runs for future updates.


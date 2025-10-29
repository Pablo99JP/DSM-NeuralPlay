DSM-NeuralPlay
================

Overview
--------
This repository contains a Clean Architecture / DDD scaffold for the "NeuralPlay" domain. The domain model is available in `domain.model.json` (generated from `dominio.puml`). The core is implemented in `ApplicationCore` (EN, CEN, CP, repository interfaces). There are in-memory repository implementations for local validation and a small `InitializeDb` console that seeds data using the domain layer.

Quick commands (Windows PowerShell)
----------------------------------
# Restore/Build core projects
dotnet build .\ApplicationCore\ApplicationCore.csproj

dotnet build .\InitializeDb\InitializeDb.csproj

# Run the initialization harness (in-memory mode by default)
# This program seeds domain objects and exercises CEN/CP logic.
dotnet run --project .\InitializeDb\InitializeDb.csproj

# Run tests
dotnet test .\tests\Domain.SmokeTests\Domain.SmokeTests.csproj

Notes
-----
- Currently the persistence implementations are in-memory for validation. NHibernate-based implementations are planned and tracked in the TODO.
- Password hashing in `AuthenticationCEN` uses a simple SHA256-based helper (for validation only). Replace with PBKDF2/BCrypt/Argon2 before production.
- The domain canonical model is `domain.model.json` at the repository root.

Where to look
-------------
- Domain entities and business logic: `ApplicationCore/Domain/EN` and `ApplicationCore/Domain/CEN`.
- Repository interfaces: `ApplicationCore/Domain/Repositories`.
- In-memory infrastructure: `ApplicationCore/Infrastructure/Memory`.
- Tests (smoke tests): `tests/Domain.SmokeTests`.

Next steps
----------
1) Expand unit and integration tests (in-memory + NHibernate).  
2) Implement NHibernate mappings and repositories.  
3) Make InitializeDb switchable (in-memory vs NHibernate SchemaExport).

If you want, I can now add focused unit tests for a few CENs and CPs (happy path + edge cases).
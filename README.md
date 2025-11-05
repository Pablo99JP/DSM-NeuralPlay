## DSM-NeuralPlay

Este repositorio contiene una implementación con ideas de Clean Architecture / DDD para el dominio "NeuralPlay".
El objetivo principal de este README es explicar en castellano las piezas del proyecto (clases, repositorios, operaciones CRUD, CRUD custom, CP/CEN, UnitOfWork, etc.) y dar instrucciones prácticas para ejecutar el servicio y las pruebas desde PowerShell en Windows.

### Estructura general

- `ApplicationCore/` — Núcleo del dominio: entidades (EN), CEN (componentes por entidad), CP (casos de proceso), interfaces de repositorio y lógica de negocio.
- `Infrastructure/` — Implementaciones de persistencia y utilidades.
	- `Infrastructure/NHibernate/` — repositorios NHibernate, mappings, `NHibernateHelper`.
	- `Infrastructure/Logging/` — configurador de Serilog (`SerilogConfigurator`).
- `InitializeDb/` — ejecutable auxiliar para exportar esquema y seed (modo `inmemory` o `schemaexport`).
- `tests/` — pruebas (smoke y unit).

### Piezas clave y responsabilidad

- Entidades (EN): `ApplicationCore/Domain/EN` — clases POCO que definen el modelo (p. ej. `Usuario`, `Comunidad`, `Equipo`, `Torneo`, `MiembroEquipo`, `MiembroComunidad`, `ParticipacionTorneo`).
- CEN (Componentes de Entidad): `ApplicationCore/Domain/CEN` — encapsulan lógica de negocio por entidad (validaciones, creación de objetos, invariantes). Ejemplo: `UsuarioCEN` crea usuarios con valores por defecto.
- CP (Casos de Proceso): `ApplicationCore/Domain/CP` — orquestan múltiples operaciones sobre varios repositorios y usan `IUnitOfWork` para asegurar atomicidad (ejemplo: `CrearComunidadCP`).
- Repositorios (interfaces): `ApplicationCore/Domain/Repositories` — contratos de persistencia; la interfaz base es `IRepository<T>`.
- Implementaciones en memoria: `ApplicationCore/Infrastructure/Memory` — útil para pruebas rápidas y validación sin BD.
- Implementaciones NHibernate: `Infrastructure/NHibernate` — persistencia real, mapeos y `NHibernateUnitOfWork`.

### Operaciones CRUD (contrato y semántica)

La interfaz base `IRepository<T>` define estas operaciones:

- `T? ReadById(long id)` — leer por id.
- `IEnumerable<T> ReadAll()` — listar todo.
- `IEnumerable<T> ReadFilter(string filter)` — búsqueda por texto (implementación en memoria hace reflexión sobre `string` properties; NHibernate puede mapear a consultas más eficientes).
- `void New(T entity)` — crear/insertar (en memoria asigna id si existe propiedad `Id*`).
- `void Modify(T entity)` — actualizar por id.
- `void Destroy(long id)` — eliminar.

### CRUD custom y reglas de negocio ya implementadas

- Usuario: `UsuarioCEN.NewUsuario(...)` establece `EstadoCuenta` a `ACTIVA` por defecto.
- MiembroEquipo / MiembroComunidad: al crear no se asigna `FechaBaja` (queda null); la baja es un flujo aparte.
- SolicitudIngreso: `SolicitudIngresoCEN.NewSolicitudIngreso(...)` evita crear la solicitud si el usuario ya está en un equipo de la comunidad objetivo (lanza `InvalidOperationException`).

### ReadFilter y consultas optimizadas (Option A)

Además del `ReadFilter` genérico existen repositorios especializados con métodos optimizados:

- `IParticipacionTorneoRepository`:
	- `GetEquiposByTorneo(long idTorneo)`
	- `GetTorneosByEquipo(long idEquipo)`
- `IMiembroEquipoRepository`:
	- `GetUsuariosByEquipo(long idEquipo)`
- `IMiembroComunidadRepository`:
	- `GetUsuariosByComunidad(long idComunidad)`

Estos métodos permiten que NHibernate ejecute consultas eficientes y que las implementaciones en memoria reproduzcan la semántica para tests.

### Transacciones / UnitOfWork

- `IUnitOfWork` agrupa cambios y expone `SaveChanges()`.
- Implementaciones:
	- `NHibernateUnitOfWork` — controla la sesión y transacciones reales.
	- `InMemoryUnitOfWork` — no-op para tests.

### Autenticación / Login

- `AuthenticationCEN.Login(nick, password)` está implementado; busca usuario por nick (`IUsuarioRepository.ReadByNick`) y verifica contraseña usando `PasswordHasher` (PBKDF2 en código actual). Revisar parámetros antes de usar en producción.

### InitializeDb: modos, flags y ejemplos

`InitializeDb` puede usarse en modo `inmemory` (validación local) o `schemaexport` (exportar esquema y opcionalmente seed). Flags principales:

- `--mode=<inmemory|schemaexport>` (default: `inmemory`)
- `--seed` — ejecutar seed idempotente.
- `--db-name=<name>` — nombre de la BD (por defecto `ProjectDatabase`).
- `--force-drop` y `--confirm` — para eliminar MDF existente de forma controlada.
- `--data-dir=<path>` — directorio donde escribir artifacts (logs, mdf, project.db).
- `--log-file=<path>` — ruta de log para Serilog.
- `--verbose` o `-v` — para salida más detallada.

Ejemplos en PowerShell:

```powershell
# Validación en memoria (no toca disco)
dotnet run --project .\InitializeDb\InitializeDb.csproj

# Exportar esquema y seed con confirmación
dotnet run --project .\InitializeDb\InitializeDb.csproj -- --mode=schemaexport --db-name=ProjectDatabase --force-drop --confirm --seed --data-dir=InitializeDb/Data --log-file=InitializeDb/Data/init.log --verbose
```

### Logging y variables de entorno

Serilog está centralizado en `Infrastructure/Logging/SerilogConfigurator`. Puedes controlarlo con:

- `LOG_FILE` — ruta a archivo de log.
- `LOG_LEVEL` — nivel deseado (`Debug`, `Information`, `Warning`, ...).
- `LOG_VERBOSE=true` — atajo para `Debug`.

El CLI pasa `--log-file` a este configurador; en tests se usa `InitializeDbService.RunAsync(...)` y se puede capturar la salida mediante un `TextWriter`.

### Tests y ejecución

Comandos rápidos (PowerShell):

```powershell
dotnet restore
dotnet build ./DSM-NeuralPlay.sln
dotnet test ./DSM-NeuralPlay.sln

# Ejecutar solo UnitTests
dotnet test ./tests/UnitTests/UnitTests.csproj

# Ejecutar solo Smoke tests
dotnet test ./tests/Domain.SmokeTests/Domain.SmokeTests.csproj
```

### Registro de implementaciones (DI)

Durante el seed (`InitializeDbService`) el contenedor `IServiceCollection` registra tanto las implementaciones NHibernate como las en memoria (para la ruta de validación). Si añades nuevos repositorios, sigue el patrón: interfaz en `ApplicationCore/Domain/Repositories`, implementaciones en NHibernate y en memoria, y registrar en DI (ej. `InitializeDbService`).

### Problemas conocidos y consejos

- El flujo de `schemaexport` intenta usar LocalDB; si LocalDB no está disponible o la DB con el mismo nombre ya existe, se hace fallback a SQLite (`project.db`) en `--data-dir`.
- Tests que leen archivos de log usan esperas/reintentos para tolerar escrituras asíncronas del sink de Serilog; en entornos muy lentos aumenta timeouts si detectas flakes.
- Mantén las dependencias actualizadas y revisa los avisos de SCA/Dependabot. Se actualizó Moq a una versión sin la advertencia previa.

### Dónde mirar (referencia rápida)

- Entidades: `ApplicationCore/Domain/EN/`
- CENs: `ApplicationCore/Domain/CEN/`
- CPs: `ApplicationCore/Domain/CP/`
- Repositorios (contratos): `ApplicationCore/Domain/Repositories/`
- Repositorios en memoria: `ApplicationCore/Infrastructure/Memory/`
- Repositorios NHibernate: `Infrastructure/NHibernate/`
- InitializeDb: `InitializeDb/InitializeDbService.cs`

Si quieres, puedo generar un script PowerShell de comprobación (restore/build/test/schemaexport) o crear ejemplos concretos de uso de `AuthenticationCEN.Login` y de un CP transaccional. Indica qué prefieres y lo añado.

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

### ReadFilter y consultas optimizadas

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

`InitializeDb` es un proyecto ejecutable que inicializa la base de datos y ejecuta el seed de datos. Por defecto está configurado para trabajar con **SQL Server LocalDB** y ejecutar automáticamente el seed.

#### Configuración por defecto

Cuando ejecutas `InitializeDb` sin argumentos:
```powershell
cd InitializeDb
dotnet run
```

El sistema automáticamente:
1. **Modo**: `schemaexport` (persistencia en base de datos real)
2. **Seed**: Activado (inserta datos de prueba)
3. **Base de datos**: `ProjectDatabase` en LocalDB
4. **Ubicación**: `InitializeDb/Data/ProjectDatabase.mdf`

#### Flujo de ejecución completo

Cuando ejecutas `InitializeDb`, el sistema sigue este flujo:

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. ANÁLISIS DE ARGUMENTOS                                      │
│    - Modo: schemaexport (por defecto)                          │
│    - Seed: true (activado por defecto)                         │
│    - DB Name: ProjectDatabase                                   │
│    - Data Dir: InitializeDb/Data                               │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 2. CONFIGURACIÓN DE LOGGING                                     │
│    - Inicializa Serilog vía SerilogConfigurator               │
│    - Sinks: Console + Archivo (si --log-file especificado)    │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 3. CREACIÓN DE BASE DE DATOS (LocalDB)                         │
│    - Lee NHibernate.cfg.xml                                     │
│    - Construye configuración de NHibernate                     │
│    - Verifica existencia de ProjectDatabase.mdf               │
│    - Si existe y NO hay --force-drop: ERROR y termina         │
│    - Si --force-drop + --confirm: Elimina MDF/LDF existentes  │
│    - Crea nueva base de datos en LocalDB                       │
│    - Ejecuta SchemaExport de NHibernate                        │
│      → Genera y ejecuta DDL SQL                                │
│      → Crea 22 tablas: Usuario, Comunidad, Equipo,            │
│        MiembroComunidad, MiembroEquipo, Torneo,               │
│        ParticipacionTorneo, PropuestaTorneo, VotoTorneo,      │
│        Publicacion, Comentario, Reaccion, Perfil,             │
│        PerfilJuego, Juego, ChatEquipo, MensajeChat,           │
│        Invitacion, Notificacion, Sesion,                      │
│        SolicitudIngreso, NHibernateUniqueKey                  │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 4. SEED DE DATOS (si --seed está activo, por defecto SÍ)       │
│    A. Construye IServiceCollection con DI:                     │
│       - SessionFactory de NHibernate                           │
│       - Todos los repositorios NHibernate                      │
│       - IUnitOfWork (NHibernateUnitOfWork)                     │
│       - Todos los CEN (UsuarioCEN, ComunidadCEN, etc.)        │
│                                                                 │
│    B. SEED IDEMPOTENTE (verifica antes de insertar):          │
│       ┌─────────────────────────────────────────────────┐     │
│       │ Usuarios (si no existen por nick):             │     │
│       │   • alice (alice@example.com)                  │     │
│       │   • bob (bob@example.com)                      │     │
│       │   → Contraseñas hasheadas con PBKDF2          │     │
│       └─────────────────────────────────────────────────┘     │
│                      ↓                                          │
│       ┌─────────────────────────────────────────────────┐     │
│       │ Comunidad (si no existe):                      │     │
│       │   • Gamers (Comunidad de prueba)              │     │
│       └─────────────────────────────────────────────────┘     │
│                      ↓                                          │
│       ┌─────────────────────────────────────────────────┐     │
│       │ Equipo (si no existe):                         │     │
│       │   • TeamA                                      │     │
│       └─────────────────────────────────────────────────┘     │
│                      ↓                                          │
│       ┌─────────────────────────────────────────────────┐     │
│       │ Relaciones:                                    │     │
│       │   • MiembroComunidad (alice → Gamers)         │     │
│       │     Rol: LIDER, Estado: ACTIVA                │     │
│       └─────────────────────────────────────────────────┘     │
│                      ↓                                          │
│    C. Llama a uow.SaveChanges() para persistir                │
│    D. Cierra SessionFactory limpiamente                        │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 5. FINALIZACIÓN                                                 │
│    - Flush de logs de Serilog                                  │
│    - Log final: "InitializeDb completed"                       │
│    - Return code 0 (éxito) o non-zero (error)                 │
└─────────────────────────────────────────────────────────────────┘
```

#### Flujo alternativo: Modo en memoria

Si ejecutas con `--mode=inmemory`:

```powershell
dotnet run -- --mode=inmemory
```

El flujo cambia a:

```
1. Crea repositorios InMemory (ApplicationCore/Infrastructure/Memory)
2. Instancia todos los CEN con repositorios en memoria
3. Instancia todos los CP (Casos de Proceso)
4. Ejecuta seed completo en memoria:
   → Crea 3 usuarios (alice, bob, charlie)
   → Crea 2 comunidades (Gamers, Devs)
   → Crea 2 equipos (TeamA, TeamB)
   → Crea publicaciones, comentarios, reacciones
   → Crea perfiles, juegos, perfil-juegos
   → Crea torneos, propuestas, participaciones, votos
   → Crea membresías, invitaciones, mensajes, notificaciones
5. Invoca TODOS los métodos de TODOS los CEN/CP:
   → 20 CENs: New, ReadAll, ReadOID, Modify, ReadFilter
   → 4 CPs: Ejecutar() con transacciones
   → Métodos custom: AddComentario, AprobarSiVotosUnanimes,
     PromocionarAModerador, BanearMiembroEquipo, etc.
6. Imprime resultados con checkmarks (✓) en consola
7. NO persiste nada (solo validación)
```

#### Optimizaciones de rendimiento

**Filtrado en SQL (no en memoria)**

Todos los `ReadFilter` de los repositorios NHibernate utilizan **LINQ to NHibernate**, que traduce las expresiones a consultas SQL optimizadas:

```csharp
// Ejemplo en NHibernateComunidadRepository
public IEnumerable<Comunidad> ReadFilter(string filter)
{
    if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
    var f = filter.ToLowerInvariant();
    // ✅ Esto se traduce a SQL:
    // SELECT * FROM Comunidad 
    // WHERE LOWER(Nombre) LIKE '%filter%' OR LOWER(Descripcion) LIKE '%filter%'
    return _session.Query<Comunidad>()
        .Where(c => c.Nombre.ToLower().Contains(f) || 
                    (c.Descripcion != null && c.Descripcion.ToLower().Contains(f)))
        .ToList();
}
```

**Consultas especializadas optimizadas**:
- `GetUsuariosByEquipo(idEquipo)` → `SELECT DISTINCT u.* FROM MiembroEquipo JOIN Usuario...`
- `GetEquiposByTorneo(idTorneo)` → `SELECT DISTINCT e.* FROM ParticipacionTorneo JOIN Equipo...`
- `ReadByEmail(email)` → `SELECT TOP 1 * FROM Usuario WHERE LOWER(CorreoElectronico) = @email`

Beneficios:
- ✅ Solo se cargan los registros necesarios
- ✅ SQL Server usa índices automáticamente
- ✅ Menor uso de memoria del backend
- ✅ Mayor velocidad en conjuntos de datos grandes

#### Flags disponibles (CLI)

- `--mode=<inmemory|schemaexport>` — Modo de ejecución (default: `schemaexport`)
- `--seed` — Ejecutar seed idempotente (default: activado)
- `--db-name=<name>` — Nombre de la BD (default: `ProjectDatabase`)
- `--data-dir=<path>` — Directorio de artifacts (default: `InitializeDb/Data`)
- `--force-drop` — Permite eliminar MDF existente (⚠️ destructivo)
- `--confirm` — Confirmación requerida con `--force-drop`
- `--log-file=<path>` — Ruta de log para Serilog
- `--verbose` o `-v` — Salida detallada (nivel Debug)
- `--target-connection=<cadena>` — Cadena de conexión personalizada
- `--dialect=<dialecto>` — Dialecto NHibernate personalizado

#### Ejemplos de uso en PowerShell

**Ejecución estándar (LocalDB + Seed):**
```powershell
cd InitializeDb
dotnet run
```

**Recrear base de datos desde cero:**
```powershell
dotnet run -- --force-drop --confirm
```

**Validación rápida en memoria (sin tocar disco):**
```powershell
dotnet run -- --mode=inmemory
```

**Con logs detallados:**
```powershell
dotnet run -- --verbose --log-file=./Data/init.log
```

**Conectar a SQL Server remoto:**
```powershell
dotnet run -- --target-connection="Server=mi-servidor;Database=MiDB;User Id=usuario;Password=clave;" --dialect="NHibernate.Dialect.MsSql2012Dialect"
```

**Base de datos personalizada:**
```powershell
dotnet run -- --db-name=MiBaseDeDatos --data-dir=./MiData
```

#### Verificar resultados

**Listar tablas creadas:**
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ProjectDatabase -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"
```

**Ver datos del seed:**
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ProjectDatabase -Q "SELECT IdUsuario, Nick, CorreoElectronico FROM Usuario"
```

**Ver membresías:**
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ProjectDatabase -Q "SELECT m.IdMiembroComunidad, u.Nick, c.Nombre, m.Rol FROM MiembroComunidad m JOIN Usuario u ON m.IdUsuario = u.IdUsuario JOIN Comunidad c ON m.IdComunidad = c.IdComunidad"
```

#### Fallback automático

Si LocalDB no está disponible o falla, el sistema automáticamente:
1. Intenta crear base de datos SQLite en `<data-dir>/project.db`
2. Usa `SQLiteDialect` y `SQLite20Driver`
3. Continúa con el mismo flujo de seed

Esto es útil para:
- ✅ CI/CD en entornos sin LocalDB
- ✅ Desarrollo en sistemas sin SQL Server
- ✅ Tests de integración portables

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

# InitializeDb

Este proyecto contiene el inicializador reproducible para crear el esquema de la base de datos y opcionalmente ejecutar un seed idempotente.

Uso principal

Desde la raíz del repositorio:

```powershell
Push-Location .\InitializeDb
dotnet run --project .\InitializeDb.csproj -- --mode=schemaexport --data-dir=./Data --log-file=./Data/init.log
Pop-Location
```

Flags disponibles (CLI)
- `--mode=`: `inmemory` (por defecto) o `schemaexport`. En `schemaexport` se exporta el esquema a LocalDB o SQLite.
- `--seed`: ejecutar el seed idempotente después de schema export (opcional).
- `--data-dir=`: carpeta donde colocar artefactos (MDF, LDF, project.db, logs). Por defecto `InitializeDb/Data`.
- `--log-file=`: ruta de fichero de log para el sink de Serilog.
- `--verbose` / `-v`: habilita salida más verbosa (debug).
- `--force-drop`: permite eliminar MDF existente (acción destructiva).
- `--confirm`: necesario junto a `--force-drop` para confirmar la acción destructiva.
- `--db-name=`: nombre de la base de datos a crear/usar (por defecto `ProjectDatabase`).

Variables de entorno soportadas
- `LOG_FILE`: alternativa a `--log-file` — usada por la configuración centralizada `SerilogConfigurator`.
- `LOG_VERBOSE`: si `true`, habilita nivel Debug.
- `LOG_LEVEL`: asigna nivel Serilog explícito (`Debug`, `Information`, `Warning`, `Error`). Tiene prioridad sobre `LOG_VERBOSE`.
- `LOCALDB_AVAILABLE`: (usada en CI) indicadora para ejecutar integraciones contra LocalDB.

Try it (local)
----------------

Abra PowerShell en la raíz del repo y ejecute:

```powershell
Push-Location .\InitializeDb
dotnet run --project .\InitializeDb.csproj -- --mode=schemaexport --seed --data-dir=./Data --log-file=./Data/init.log --db-name=local_test_db --force-drop --confirm
Pop-Location
```

Notas útiles
-----------
- En CI la acción exporta `LOG_FILE`, `LOG_VERBOSE` y `LOG_LEVEL` para controlar la salida de Serilog y reunir artefactos.
- Si deseas una prueba no destructiva usa `--mode=inmemory` para validar la lógica de seeding en memoria.
- Los artefactos de la ejecución se encuentran en `--data-dir` y pueden incluir `*.mdf`, `*_log.ldf`, `project.db` y el fichero de log.

Salida / artefactos
- Si se usa LocalDB y la creación tiene éxito, se dejará un fichero MDF en `<data-dir>/<db-name>.mdf` y el LDF `<db-name>_log.ldf`.
- Si LocalDB no está disponible o falla, se genera `project.db` (SQLite) en la carpeta `data-dir`.
- Los logs (console + file) se pueden encontrar en la ruta indicada por `--log-file` o `LOG_FILE`.

Notas
- El seed es opcional y está diseñado para ser idempotente. Utiliza los CENs y repositorios de `ApplicationCore`.
- En CI la workflow exporta `LOG_FILE`, `LOG_VERBOSE` y `LOG_LEVEL` para que los artefactos subidos contengan los logs esperados.

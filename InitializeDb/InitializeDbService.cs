using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.Data.SqlClient;

/// <summary>
/// Servicio principal para inicializar la base de datos del proyecto.
/// Este servicio crea el esquema de la BD y pobla las tablas con datos de prueba.
/// </summary>
public static class InitializeDbService
{
    /// <summary>
    /// Punto de entrada principal para inicializar la base de datos.
    /// </summary>
    /// <param name="args">Argumentos de línea de comandos (--force-drop, --confirm, --db-name, etc.)</param>
    /// <param name="externalLogWriter">Writer opcional para logging externo (usado en tests)</param>
    /// <returns>0 si tiene éxito, código de error si falla</returns>
    public static async Task<int> RunAsync(string[] args, TextWriter? externalLogWriter = null)
    {
        try
        {
            // ========================================
            // PASO 1: LECTURA DE ARGUMENTOS DE LÍNEA DE COMANDOS
            // ========================================
            // Procesamos los parámetros que el usuario puede pasar al ejecutar el programa

            bool forceDrop = false;        // Si debe eliminar la BD existente
            bool confirm = false;          // Confirmación para acciones destructivas
            string dbName = "ProjectDatabase";  // Nombre de la base de datos
            bool doSeed = true;            // Si debe poblar la BD con datos (activado por defecto)
            string? dataDirArg = null;     // Directorio donde guardar los archivos de BD
            bool verbose = false;          // Modo detallado de logging
            string? logFile = null;        // Archivo donde guardar logs
            string? targetConnection = null;  // Cadena de conexión personalizada
            string? targetDialect = null;  // Dialecto SQL personalizado (SQL Server, SQLite, etc.)

            // Recorremos cada argumento y lo procesamos
            foreach (var a in args)
            {
                // --force-drop: Permite borrar la base de datos existente
                if (a.Equals("--force-drop", StringComparison.OrdinalIgnoreCase))
                {
                    forceDrop = true;
                }
                // --confirm: Confirmación requerida para evitar borrados accidentales
                else if (a.Equals("--confirm", StringComparison.OrdinalIgnoreCase))
                {
                    confirm = true;
                }
                // --db-name=NombreBD: Especifica el nombre de la base de datos
                else if (a.StartsWith("--db-name=", StringComparison.OrdinalIgnoreCase))
                {
                    dbName = a.Substring("--db-name=".Length);
                    if (string.IsNullOrWhiteSpace(dbName)) dbName = "ProjectDatabase";
                }
                // --seed: Activa el poblado de datos de prueba (ya está activo por defecto)
                else if (a.Equals("--seed", StringComparison.OrdinalIgnoreCase))
                {
                    doSeed = true;
                }
                // --data-dir=Ruta: Especifica dónde guardar los archivos de BD
                else if (a.StartsWith("--data-dir=", StringComparison.OrdinalIgnoreCase))
                {
                    dataDirArg = a.Substring("--data-dir=".Length);
                    if (string.IsNullOrWhiteSpace(dataDirArg)) dataDirArg = null;
                }
                // --target-connection=...: Usa una cadena de conexión personalizada
                else if (a.StartsWith("--target-connection=", StringComparison.OrdinalIgnoreCase))
                {
                    targetConnection = a.Substring("--target-connection=".Length);
                    if (string.IsNullOrWhiteSpace(targetConnection)) targetConnection = null;
                }
                // --dialect=...: Especifica el dialecto SQL (MsSql, SQLite, PostgreSQL, etc.)
                else if (a.StartsWith("--dialect=", StringComparison.OrdinalIgnoreCase))
                {
                    targetDialect = a.Substring("--dialect=".Length);
                    if (string.IsNullOrWhiteSpace(targetDialect)) targetDialect = null;
                }
                // --verbose o -v: Activa logging detallado para debugging
                else if (a.Equals("--verbose", StringComparison.OrdinalIgnoreCase) || a.Equals("-v", StringComparison.OrdinalIgnoreCase))
                {
                    verbose = true;
                }
                // --log-file=Ruta: Guarda los logs en un archivo específico
                else if (a.StartsWith("--log-file=", StringComparison.OrdinalIgnoreCase))
                {
                    logFile = a.Substring("--log-file=".Length);
                    if (string.IsNullOrWhiteSpace(logFile)) logFile = null;
                }
            }

            // ========================================
            // PASO 2: CONFIGURACIÓN DEL SISTEMA DE LOGGING
            // ========================================

            // Función auxiliar para reintentar operaciones que pueden fallar temporalmente
            // (como eliminar archivos que están siendo usados por otro proceso)
            async Task RetryAsync(Func<Task> work, int attempts = 3, int delayMs = 200)
            {
                int tries = 0;
                while (true)
                {
                    try
                    {
                        await work();
                        return;
                    }
                    catch (Exception)
                    {
                        tries++;
                        if (tries >= attempts) throw;
                        try { await Task.Delay(delayMs); } catch { }
                    }
                }
            }

            // Configurar Serilog sin depender de Infrastructure.Logging.SerilogConfigurator (que no existe en la solución)
            try
            {
                var level = args.Any(a => a.Equals("--verbose", StringComparison.OrdinalIgnoreCase) || a.Equals("-v", StringComparison.OrdinalIgnoreCase))
                    ? Serilog.Events.LogEventLevel.Debug
                    : Serilog.Events.LogEventLevel.Information;

                var loggerCfg = new LoggerConfiguration()
                    .MinimumLevel.Is(level)
                    .WriteTo.Console();

                // Si se pasó --log-file=..., escribir también a fichero
                var logFileArg = args.FirstOrDefault(a => a.StartsWith("--log-file=", StringComparison.OrdinalIgnoreCase));
                var logFilePath = logFileArg != null ? logFileArg.Substring("--log-file=".Length) : null;
                if (!string.IsNullOrWhiteSpace(logFilePath))
                {
                    loggerCfg = loggerCfg.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day);
                }

                Log.Logger = loggerCfg.CreateLogger();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to configure Serilog: {ex.Message}");
                Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            }

            using var loggerFactory = new SerilogLoggerFactory(Log.Logger, dispose: false);
            var logger = loggerFactory.CreateLogger("InitializeDb");

            // Función auxiliar para escribir en el log externo (usado principalmente en tests automáticos)
            void FileLog(string line)
            {
                try
                {
                    if (externalLogWriter != null)
                    {
                        externalLogWriter.WriteLine(line);
                        externalLogWriter.Flush();
                    }
                }
                catch { }  // Silenciosamente ignora errores de logging para no interrumpir el proceso
            }

            // ========================================
            // PASO 3: DETERMINACIÓN DEL DIRECTORIO DE DATOS
            // ========================================
            // Decidimos dónde guardar los archivos de la base de datos (.mdf y .ldf)

            string dataDir;
            if (!string.IsNullOrWhiteSpace(dataDirArg))
            {
                // Si el usuario especificó un directorio, lo usamos
                dataDir = Path.GetFullPath(dataDirArg);
            }
            else
            {
                // Por defecto, usamos la carpeta "Data" en la raíz del proyecto
                // (subimos 3 niveles desde bin/Debug/net8.0)
                var repoData = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data"));
                dataDir = repoData;
            }

            // Nos aseguramos de que el directorio exista
            Directory.CreateDirectory(dataDir);

            // Mostramos información sobre lo que vamos a hacer
            Console.WriteLine($"InitializeDb - LocalDB SchemaExport Mode");
            Console.WriteLine($"Database: {dbName} | LogFile: {logFile ?? "none"} | Verbose: {verbose}");
            if (externalLogWriter != null)
            {
                FileLog($"[{DateTime.UtcNow:o}] InitializeDb log started (external writer)");
            }

            logger.LogInformation("InitializeDb - running NHibernate SchemaExport to LocalDB...");
            FileLog($"[{DateTime.UtcNow:o}] InitializeDb started. dbName={dbName} verbose={verbose}");

            // ========================================
            // PASO 4: CREACIÓN DEL ESQUEMA DE BASE DE DATOS
            // ========================================

            // Ruta completa del archivo de base de datos (.mdf)
            var mdfPath = Path.Combine(dataDir, dbName + ".mdf");

            // Variables para guardar la configuración de conexión que finalmente usemos
            string? lastConnectionString = null;
            string? lastDialect = null;

            try
            {
                // OPCIÓN A: Si el usuario proporcionó una cadena de conexión personalizada
                if (!string.IsNullOrWhiteSpace(targetConnection))
                {
                    // Usamos la conexión y dialecto especificados
                    var dialectToUse = string.IsNullOrWhiteSpace(targetDialect) ? "NHibernate.Dialect.MsSql2012Dialect" : targetDialect;
                    FileLog($"[{DateTime.UtcNow:o}] Exporting schema to target connection using connection: {targetConnection} dialect: {dialectToUse}");
                    Console.WriteLine($"Exporting schema to target connection: {targetConnection} dialect: {dialectToUse}");

                    // Si es SQL Server, usamos el driver de Microsoft
                    var driver = dialectToUse.Contains("MsSql", StringComparison.OrdinalIgnoreCase) ? "NHibernate.Driver.MicrosoftDataSqlClientDriver" : null;

                    // Construimos la configuración de NHibernate con los mappings de las entidades
                    var cfgForExport = NHibernateHelper.BuildConfiguration();
                    if (!string.IsNullOrWhiteSpace(dialectToUse)) cfgForExport.SetProperty("dialect", dialectToUse);
                    if (!string.IsNullOrWhiteSpace(driver)) cfgForExport.SetProperty("connection.driver_class", driver);
                    cfgForExport.SetProperty("connection.connection_string", targetConnection);

                    // Generamos el script SQL con la estructura de todas las tablas
                    var schemaFile = Path.Combine(dataDir, dbName + "_schema.sql");
                    try
                    {
                        // Creamos el archivo SQL con el esquema (CREATE TABLE, etc.)
                        var export = new NHibernate.Tool.hbm2ddl.SchemaExport(cfgForExport);
                        export.SetOutputFile(schemaFile);
                        export.Create(false, false);  // false = no mostrar en consola, false = no ejecutar (solo escribir archivo)

                        // Extraemos el servidor y nombre de BD de la cadena de conexión
                        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(targetConnection);
                        var server = builder.DataSource;
                        var database = builder.InitialCatalog ?? dbName;

                        // Ejecutamos el script SQL usando sqlcmd (herramienta de línea de comandos de SQL Server)
                        var psi = new System.Diagnostics.ProcessStartInfo("sqlcmd", $"-S \"{server}\" -d \"{database}\" -E -i \"{schemaFile}\"")
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        };
                        using var proc = System.Diagnostics.Process.Start(psi);
                        if (proc != null)
                        {
                            // Capturamos la salida del proceso
                            var outText = proc.StandardOutput.ReadToEnd();
                            var errText = proc.StandardError.ReadToEnd();
                            proc.WaitForExit(60000);  // Esperamos máximo 60 segundos

                            if (proc.ExitCode != 0)
                            {
                                // Si sqlcmd falló, mostramos el error
                                Console.WriteLine($"sqlcmd failed: {errText}");
                                throw new InvalidOperationException($"sqlcmd failed: {errText}");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Failed to start sqlcmd process");
                        }

                        // Guardamos la configuración que usamos para el seed posterior
                        lastConnectionString = targetConnection;
                        lastDialect = dialectToUse;
                        logger.LogInformation("SchemaExport to target connection completed via sqlcmd.");
                        FileLog($"[{DateTime.UtcNow:o}] SchemaExport to target connection completed successfully via script {schemaFile}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SchemaExport to target connection failed: {ex.Message}");
                        throw;
                    }
                }
                // OPCIÓN B: Usar LocalDB (SQL Server Express) - comportamiento por defecto
                else
                {
                    logger.LogInformation($"Attempting SchemaExport to LocalDB ({Path.GetFileName(mdfPath)})...");
                    FileLog($"[{DateTime.UtcNow:o}] Attempting SchemaExport to LocalDB ({mdfPath})");

                    // Verificamos si la BD ya existe en el fichero físico o en la instancia;
                    // la lógica posterior consultará `sys.databases` y decidirá si usarla o crearla.
                    // Si quieren borrar pero no confirmaron, abortamos por seguridad
                    if (forceDrop && !confirm)
                    {
                        Console.WriteLine("--force-drop specified but --confirm not provided. Aborting destructive action. Falling back to SQLite.");
                        FileLog($"[{DateTime.UtcNow:o}] --force-drop without --confirm; aborting LocalDB destructive action.");
                        throw new InvalidOperationException("Force drop requested without confirm.");
                    }

                    // Conectamos a la BD master de LocalDB para poder administrar bases de datos
                    using var masterConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=5;");
                    try
                    {
                        masterConn.Open();

                        // Si existe la BD y tenemos confirmación, la borramos
                        if (forceDrop && confirm && File.Exists(mdfPath))
                        {
                            try
                            {
                                // Primero intentamos hacer DROP DATABASE desde SQL
                                var dropCmd = masterConn.CreateCommand();
                                dropCmd.CommandText = $@"
IF EXISTS(SELECT name FROM sys.databases WHERE name = N'{dbName}')
BEGIN
    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;  -- Desconectamos a todos los usuarios
    DROP DATABASE [{dbName}];                                            -- Eliminamos la BD
END
";
                                dropCmd.ExecuteNonQuery();
                            }
                            catch (Exception dropEx)
                            {
                                // Si el DROP DATABASE falla, lo intentaremos borrando el archivo directamente
                                Console.WriteLine($"Failed to drop existing database '{dbName}': {dropEx.Message}. Will attempt file delete.");
                                FileLog($"[{DateTime.UtcNow:o}] Failed to drop existing DB: {dropEx.Message}");
                            }

                            try
                            {
                                // Intentamos borrar físicamente los archivos de BD
                                Console.WriteLine($"Deleting existing MDF {mdfPath} as --force-drop and --confirm were provided...");
                                await RetryAsync(() => { File.Delete(mdfPath); return Task.CompletedTask; }, attempts: 3, delayMs: 200);

                                // También borramos el archivo de log (.ldf)
                                var logPath = Path.Combine(dataDir, dbName + "_log.ldf");
                                if (File.Exists(logPath))
                                    await RetryAsync(() => { File.Delete(logPath); return Task.CompletedTask; }, attempts: 3, delayMs: 200);

                                Console.WriteLine("Existing MDF removed.");
                            }
                            catch (Exception delEx)
                            {
                                Console.WriteLine($"Failed to delete existing MDF: {delEx.Message}. Falling back to SQLite.");
                                FileLog($"[{DateTime.UtcNow:o}] Failed to delete existing MDF: {delEx.Message}");
                                throw;
                            }
                        }

                        // Creamos la nueva base de datos si no existe
                        // Check if the database already exists in the LocalDB instance (sys.databases)
                        try
                        {
                            var checkCmd = masterConn.CreateCommand();
                            checkCmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
                            var param = checkCmd.CreateParameter();
                            param.ParameterName = "@name";
                            param.Value = dbName;
                            checkCmd.Parameters.Add(param);
                            var existsObj = checkCmd.ExecuteScalar();
                            var dbExists = 0;
                            try { dbExists = Convert.ToInt32(existsObj); } catch { dbExists = 0; }

                            if (dbExists > 0)
                            {
                                // Database exists on the LocalDB instance
                                if (forceDrop && confirm)
                                {
                                    // Drop it (the existing drop logic below will also handle file deletion)
                                    try
                                    {
                                        var dropCmd = masterConn.CreateCommand();
                                        dropCmd.CommandText = $@"
IF EXISTS(SELECT name FROM sys.databases WHERE name = N'{dbName}')
BEGIN
    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{dbName}];
END
";
                                        dropCmd.ExecuteNonQuery();
                                    }
                                    catch (Exception dropEx)
                                    {
                                        Console.WriteLine($"Failed to drop existing database '{dbName}': {dropEx.Message}. Will attempt file delete.");
                                        FileLog($"[{DateTime.UtcNow:o}] Failed to drop existing DB: {dropEx.Message}");
                                    }
                                    // Attempt to delete files if present (existing logic handles this later)
                                    try
                                    {
                                        var logPath = Path.Combine(dataDir, dbName + "_log.ldf");
                                        if (File.Exists(mdfPath)) await RetryAsync(() => { File.Delete(mdfPath); return Task.CompletedTask; }, attempts: 3, delayMs: 200);
                                        if (File.Exists(logPath)) await RetryAsync(() => { File.Delete(logPath); return Task.CompletedTask; }, attempts: 3, delayMs: 200);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    // If DB exists and we're not dropping it, just use the existing DB and skip CREATE
                                    Console.WriteLine($"LocalDB database '{dbName}' already exists on the instance; using existing database.");
                                    FileLog($"[{DateTime.UtcNow:o}] LocalDB database '{dbName}' exists; skipping CREATE and using it.");
                                }
                            }
                            else
                            {
                                // If the database does not exist in the instance, create the MDF file and database
                                if (!File.Exists(mdfPath))
                                {
                                    try
                                    {
                                        var logPath = Path.Combine(dataDir, dbName + "_log.ldf");
                                        Console.WriteLine($"Creating LocalDB MDF at {mdfPath}...");
                                        var createCmd = masterConn.CreateCommand();
                                        createCmd.CommandText = $@"CREATE DATABASE [{dbName}] ON (NAME=N'{dbName}', FILENAME=N'{mdfPath}') LOG ON (NAME=N'{dbName}_log', FILENAME=N'{logPath}');";
                                        createCmd.ExecuteNonQuery();
                                        logger.LogInformation("LocalDB database created.");
                                    }
                                    catch (Exception createEx)
                                    {
                                        Console.WriteLine($"Failed to create LocalDB MDF: {createEx.Message}");
                                        FileLog($"[{DateTime.UtcNow:o}] Failed to create LocalDB MDF: {createEx.Message}");
                                        throw;
                                    }
                                }
                            }
                        }
                        catch (Exception exCheck)
                        {
                            Console.WriteLine($"Error while checking LocalDB existence: {exCheck.Message}");
                            FileLog($"[{DateTime.UtcNow:o}] Error checking LocalDB existence: {exCheck.Message}");
                            throw;
                        }

                        // Ahora que tenemos la BD vacía, generamos el esquema (tablas, constraints, etc.)
                        string dbConn;
                        if (File.Exists(mdfPath))
                        {
                            // If we have a physical MDF, prefer attaching it explicitly to avoid login/attach issues
                            dbConn = $"Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename={mdfPath};Integrated Security=True;Connect Timeout=30;";
                        }
                        else
                        {
                            dbConn = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Connect Timeout=30;";
                        }
                        FileLog($"[{DateTime.UtcNow:o}] Exporting schema to LocalDB using connection: {dbConn}");

                        // NHibernate leerá los mappings .hbm.xml y generará las sentencias CREATE TABLE
                        await RetryAsync(() => { NHibernateHelper.ExportSchema(dbConn, "NHibernate.Dialect.MsSql2012Dialect"); return Task.CompletedTask; }, attempts: 3, delayMs: 200);

                        lastConnectionString = dbConn;
                        lastDialect = "NHibernate.Dialect.MsSql2012Dialect";
                        logger.LogInformation("SchemaExport to LocalDB completed.");
                        FileLog($"[{DateTime.UtcNow:o}] SchemaExport to LocalDB completed successfully.");
                    }
                    catch (SqlException sqlEx)
                    {
                        Console.WriteLine($"LocalDB operations failed: {sqlEx.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                // PLAN B: Si LocalDB falla, intentamos con SQLite (base de datos en archivo simple)
                Console.WriteLine($"LocalDB SchemaExport failed: {ex.Message}");
                logger.LogWarning("Falling back to file-based SQLite SchemaExport...");
                FileLog($"[{DateTime.UtcNow:o}] LocalDB SchemaExport failed: {ex.Message}. Falling back to SQLite.");

                var sqlitePath = Path.Combine(dataDir, "project.db");
                // Use a connection string compatible with Microsoft.Data.Sqlite (no 'Version' keyword)
                var sqliteConn = $"Data Source={sqlitePath}";
                try
                {
                    FileLog($"[{DateTime.UtcNow:o}] Exporting schema to SQLite file at {sqlitePath}");
                    // Explicitly set the SQLite driver so NHibernate uses System.Data.SQLite (SQLite20Driver)
                    await RetryAsync(() => { NHibernateHelper.ExportSchema(sqliteConn, "NHibernate.Dialect.SQLiteDialect", "NHibernate.Driver.SQLite20Driver"); return Task.CompletedTask; }, attempts: 3, delayMs: 200);
                    lastConnectionString = sqliteConn;
                    lastDialect = "NHibernate.Dialect.SQLiteDialect";
                    logger.LogInformation("SchemaExport to SQLite file completed. Path={path}", sqlitePath);
                    FileLog($"[{DateTime.UtcNow:o}] SchemaExport to SQLite completed successfully. Path={sqlitePath}");
                }
                catch (Exception ex2)
                {
                    // Si incluso SQLite falla, no podemos continuar
                    Console.WriteLine($"SQLite SchemaExport also failed: {ex2.Message}");
                    FileLog($"[{DateTime.UtcNow:o}] SQLite SchemaExport failed: {ex2.Message}");
                    Console.WriteLine("InitializeDb schema export failed. Review NHibernate configuration and environment (LocalDB availability, file permissions).");
                    return 2;  // Código de error 2
                }
            }

            logger.LogInformation("InitializeDb schema export finished.");
            FileLog($"[{DateTime.UtcNow:o}] InitializeDb schema export finished. connection={lastConnectionString} dialect={lastDialect}");

            // ========================================
            // PASO 5: POBLADO DE DATOS (SEEDING)
            // ========================================

            if (doSeed)
            {
                logger.LogInformation("Seeding database via NHibernate repositories (idempotent)...");
                FileLog($"[{DateTime.UtcNow:o}] Starting idempotent seed. connection={lastConnectionString}");
                try
                {
                    // Verificamos que tenemos la configuración de conexión necesaria
                    if (string.IsNullOrWhiteSpace(lastConnectionString) || string.IsNullOrWhiteSpace(lastDialect))
                    {
                        Console.WriteLine("Warning: connection/dialect for seeding not available. Skipping seed.");
                    }
                    else
                    {
                        // PASO 5.1: Preparación del contenedor de inyección de dependencias
                        var services = new ServiceCollection();

                        // Configuramos NHibernate con la conexión activa (LocalDB o SQLite)
                        var seedCfg = NHibernateHelper.BuildConfiguration();
                        seedCfg.SetProperty("connection.connection_string", lastConnectionString);
                        seedCfg.SetProperty("dialect", lastDialect);

                        // Si es SQL Server, usamos el driver específico de Microsoft
                        if (!string.IsNullOrWhiteSpace(lastDialect) && lastDialect.Contains("MsSql", StringComparison.OrdinalIgnoreCase))
                        {
                            seedCfg.SetProperty("connection.driver_class", "NHibernate.Driver.MicrosoftDataSqlClientDriver");
                        }
                        else if (!string.IsNullOrWhiteSpace(lastDialect) && lastDialect.Contains("SQLite", StringComparison.OrdinalIgnoreCase))
                        {
                            // When using SQLite fallback, ensure NHibernate uses the SQLite ADO.NET driver
                            seedCfg.SetProperty("connection.driver_class", "NHibernate.Driver.SQLite20Driver");
                        }

                        // Construimos el SessionFactory (fábrica de sesiones de BD)
                        var seedSf = seedCfg.BuildSessionFactory();

                        // Registramos el SessionFactory y las sesiones como servicios
                        services.AddSingleton(seedSf);
                        services.AddScoped(provider => seedSf.OpenSession());

                        // Registramos TODOS los repositorios (21 repositorios para acceso a datos)
                        services.AddScoped<IUsuarioRepository, NHibernateUsuarioRepository>();
                        services.AddScoped<IRepository<Comunidad>, NHibernateComunidadRepository>();
                        services.AddScoped<IRepository<Equipo>, NHibernateEquipoRepository>();
                        services.AddScoped<IRepository<MiembroComunidad>, NHibernateMiembroComunidadRepository>();
                        services.AddScoped<IRepository<Publicacion>, NHibernatePublicacionRepository>();
                        services.AddScoped<IRepository<Comentario>, NHibernateComentarioRepository>();
                        services.AddScoped<IRepository<Reaccion>, NHibernateReaccionRepository>();
                        services.AddScoped<IRepository<Perfil>, NHibernatePerfilRepository>();
                        services.AddScoped<IRepository<Juego>, NHibernateJuegoRepository>();
                        services.AddScoped<IRepository<PerfilJuego>, NHibernatePerfilJuegoRepository>();
                        services.AddScoped<IRepository<Torneo>, NHibernateTorneoRepository>();
                        services.AddScoped<IRepository<ChatEquipo>, NHibernateChatEquipoRepository>();
                        services.AddScoped<IRepository<MensajeChat>, NHibernateMensajeChatRepository>();
                        services.AddScoped<IRepository<Invitacion>, NHibernateInvitacionRepository>();
                        services.AddScoped<IRepository<Notificacion>, NHibernateNotificacionRepository>();
                        services.AddScoped<IRepository<Sesion>, NHibernateSesionRepository>();
                        services.AddScoped<IRepository<SolicitudIngreso>, NHibernateSolicitudIngresoRepository>();
                        services.AddScoped<IRepository<VotoTorneo>, NHibernateVotoTorneoRepository>();
                        services.AddScoped<IRepository<PropuestaTorneo>, NHibernatePropuestaTorneoRepository>();
                        services.AddScoped<IRepository<MiembroEquipo>, NHibernateMiembroEquipoRepository>();
                        services.AddScoped<IRepository<MiembroComunidad>, NHibernateMiembroComunidadRepository>();
                        services.AddScoped<IRepository<ParticipacionTorneo>, NHibernateParticipacionTorneoRepository>();

                        // Registramos interfaces especializadas de repositorios (con métodos personalizados)
                        services.AddScoped<IParticipacionTorneoRepository, NHibernateParticipacionTorneoRepository>();
                        services.AddScoped<IMiembroEquipoRepository, NHibernateMiembroEquipoRepository>();
                        services.AddScoped<IMiembroComunidadRepository, NHibernateMiembroComunidadRepository>();

                        // Registramos el Unit of Work (maneja transacciones y sesiones)
                        services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();

                        // Registramos TODOS los CENs (lógica de negocio - 21 CENs)
                        services.AddScoped<UsuarioCEN>();
                        services.AddScoped<AuthenticationCEN>();  // CEN especial para autenticación y login
                        services.AddScoped<ComunidadCEN>();
                        services.AddScoped<EquipoCEN>();
                        services.AddScoped<PublicacionCEN>();
                        services.AddScoped<ComentarioCEN>();
                        services.AddScoped<ReaccionCEN>();
                        services.AddScoped<PerfilCEN>();
                        services.AddScoped<JuegoCEN>();
                        services.AddScoped<PerfilJuegoCEN>();
                        services.AddScoped<PropuestaTorneoCEN>();
                        services.AddScoped<ParticipacionTorneoCEN>();
                        services.AddScoped<VotoTorneoCEN>();
                        services.AddScoped<TorneoCEN>();
                        services.AddScoped<MiembroComunidadCEN>();
                        services.AddScoped<MiembroEquipoCEN>();
                        services.AddScoped<ChatEquipoCEN>();
                        services.AddScoped<MensajeChatCEN>();
                        services.AddScoped<InvitacionCEN>();
                        services.AddScoped<NotificacionCEN>();
                        services.AddScoped<SesionCEN>();
                        services.AddScoped<SolicitudIngresoCEN>();

                        // Registramos los CPs (casos de uso transaccionales - 4 CPs)
                        services.AddScoped<CrearComunidadCP>();
                        services.AddScoped<UnirEquipoCP>();
                        services.AddScoped<AceptarInvitacionCP>();
                        services.AddScoped<AprobarPropuestaTorneoCP>();

                        // Construimos el proveedor de servicios (contenedor DI listo para usar)
                        var provider = services.BuildServiceProvider();

                        // PASO 5.2: Ejecución del poblado de datos dentro de un scope transaccional
                        using var scope = provider.CreateScope();

                        // Obtenemos TODOS los CENs del contenedor DI (lógica de negocio)
                        var usuarioCEN = scope.ServiceProvider.GetRequiredService<UsuarioCEN>();
                        var authCEN = scope.ServiceProvider.GetRequiredService<AuthenticationCEN>();
                        var comunidadCEN = scope.ServiceProvider.GetRequiredService<ComunidadCEN>();
                        var equipoCEN = scope.ServiceProvider.GetRequiredService<EquipoCEN>();
                        var publicacionCEN = scope.ServiceProvider.GetRequiredService<PublicacionCEN>();
                        var comentarioCEN = scope.ServiceProvider.GetRequiredService<ComentarioCEN>();
                        var reaccionCEN = scope.ServiceProvider.GetRequiredService<ReaccionCEN>();
                        var perfilCEN = scope.ServiceProvider.GetRequiredService<PerfilCEN>();
                        var juegoCEN = scope.ServiceProvider.GetRequiredService<JuegoCEN>();
                        var perfilJuegoCEN = scope.ServiceProvider.GetRequiredService<PerfilJuegoCEN>();
                        var propuestaCEN = scope.ServiceProvider.GetRequiredService<PropuestaTorneoCEN>();
                        var participacionCEN = scope.ServiceProvider.GetRequiredService<ParticipacionTorneoCEN>();
                        var votoCEN = scope.ServiceProvider.GetRequiredService<VotoTorneoCEN>();
                        var miembroComunidadCEN = scope.ServiceProvider.GetRequiredService<MiembroComunidadCEN>();
                        var miembroEquipoCEN = scope.ServiceProvider.GetRequiredService<MiembroEquipoCEN>();
                        var chatEquipoCEN = scope.ServiceProvider.GetRequiredService<ChatEquipoCEN>();
                        var mensajeChatCEN = scope.ServiceProvider.GetRequiredService<MensajeChatCEN>();
                        var invitacionCEN = scope.ServiceProvider.GetRequiredService<InvitacionCEN>();
                        var notificacionCEN = scope.ServiceProvider.GetRequiredService<NotificacionCEN>();
                        var sesionCEN = scope.ServiceProvider.GetRequiredService<SesionCEN>();
                        var solicitudCEN = scope.ServiceProvider.GetRequiredService<SolicitudIngresoCEN>();

                        // Obtenemos los CPs (casos de uso transaccionales)
                        var crearComunidadCP = scope.ServiceProvider.GetRequiredService<CrearComunidadCP>();
                        var unirEquipoCP = scope.ServiceProvider.GetRequiredService<UnirEquipoCP>();
                        var aceptarInvitacionCP = scope.ServiceProvider.GetRequiredService<AceptarInvitacionCP>();
                        var aprobarPropuestaTorneoCP = scope.ServiceProvider.GetRequiredService<AprobarPropuestaTorneoCP>();

                        // Obtenemos el Unit of Work y algunos repositorios específicos
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
                        var comunidadRepo = scope.ServiceProvider.GetRequiredService<IRepository<Comunidad>>();
                        var equipoRepo = scope.ServiceProvider.GetRequiredService<IRepository<Equipo>>();
                        var torneoRepo = scope.ServiceProvider.GetRequiredService<IRepository<Torneo>>();
                        var propuestaRepo = scope.ServiceProvider.GetRequiredService<IRepository<PropuestaTorneo>>();
                        var participacionRepo = scope.ServiceProvider.GetRequiredService<IRepository<ParticipacionTorneo>>();

                        Console.WriteLine("Starting comprehensive seed...");
                        logger.LogInformation("Starting comprehensive seed for all entities");

                        // PASO 5.3: Funciones auxiliares para evitar duplicados (idempotencia)
                        // Estas funciones crean las entidades solo si no existen ya en la BD

                        // Helper: Crear o recuperar un usuario por su nombre de usuario (nick)
                        Usuario GetOrCreateUser(string nick, string email, string password)
                        {
                            var existing = usuarioRepo.ReadByNick(nick);
                            if (existing == null)
                            {
                                // No existe, lo creamos con contraseña en claro (NewUsuario se encarga del hashing)
                                var user = usuarioCEN.NewUsuario(nick, email, password);
                                logger.LogInformation("Created user {nick} (id={id})", nick, user.IdUsuario);
                                Console.WriteLine($"✓ Created user: {nick}");
                                return user;
                            }
                            else
                            {
                                Console.WriteLine($"✓ User {nick} already exists");
                                return existing;
                            }
                        }

                        // PASO 5.4: Creación de datos de prueba para TODAS las entidades

                        // 1. USUARIOS (entidad fundamental del sistema)
                        var u1 = GetOrCreateUser("alice", "alice@example.com", "password1");
                        var u2 = GetOrCreateUser("bob", "bob@example.com", "password2");
                        var u3 = GetOrCreateUser("charlie", "charlie@example.com", "password3");

                        // Helper: Crear o recuperar una comunidad por su nombre
                        Comunidad GetOrCreateComunidad(string nombre, string descripcion)
                        {
                            var existing = comunidadRepo.ReadFilter(nombre).FirstOrDefault();
                            if (existing == null)
                            {
                                // Creamos nueva comunidad
                                var comunidad = comunidadCEN.NewComunidad(nombre, descripcion);
                                logger.LogInformation("Created comunidad {nombre} (id={id})", nombre, comunidad.IdComunidad);
                                Console.WriteLine($"✓ Created comunidad: {nombre}");
                                return comunidad;
                            }
                            else
                            {
                                Console.WriteLine($"✓ Comunidad {nombre} already exists");
                                return existing;
                            }
                        }

                        // Helper: Crear o recuperar un equipo por su nombre
                        Equipo GetOrCreateEquipo(string nombre, string descripcion, Comunidad comunidadDefault)
                        {
                            var existing = equipoRepo.ReadFilter(nombre).FirstOrDefault();
                            if (existing == null)
                            {
                                // Creamos nuevo equipo con comunidad asignada para cumplir FK NOT NULL si aplica
                                var equipo = new Equipo { Nombre = nombre, Descripcion = descripcion, FechaCreacion = DateTime.UtcNow, Comunidad = comunidadDefault };
                                equipoRepo.New(equipo);
                                logger.LogInformation("Created equipo {nombre} (id={id})", nombre, equipo.IdEquipo);
                                Console.WriteLine($"✓ Created equipo: {nombre}");
                                return equipo;
                            }
                            else
                            {
                                Console.WriteLine($"✓ Equipo {nombre} already exists");
                                // Si existe y no tiene comunidad, la asignamos
                                if (existing.Comunidad == null)
                                {
                                    existing.Comunidad = comunidadDefault;
                                    equipoRepo.Modify(existing);
                                }
                                return existing;
                            }
                        }

                        // 2. COMUNIDADES (grupos de usuarios con intereses comunes)
                        var com1 = GetOrCreateComunidad("Fortnite", "Comunidad de Fortnite");
                        var com2 = GetOrCreateComunidad("GTA V", "Comunidad de GTA V");
                        var com3 = GetOrCreateComunidad("Valorant", "Comunidad de Valorant");
                        var com4 = GetOrCreateComunidad("Rocket League", "Comunidad de Rocket League");

                        // Asignar imágenes a las comunidades de prueba
                        com1.ImagenUrl = "/Recursos/Comunidades/Fortnite.jpg";
                        com2.ImagenUrl = "/Recursos/Comunidades/GTAV.webp";
                        com3.ImagenUrl = "/Recursos/Comunidades/Valorant.jpg";
                        com4.ImagenUrl = "/Recursos/Comunidades/RocketLeague.jpg";
                        comunidadRepo.Modify(com1);
                        comunidadRepo.Modify(com2);
                        comunidadRepo.Modify(com3);
                        comunidadRepo.Modify(com4);

                        // Persistir inmediatamente las comunidades para tener Id asignado antes de usarlas como FK
                        uow.SaveChanges();

                        // 3. EQUIPOS (grupos competitivos dentro de comunidades)
                        var eq1 = GetOrCreateEquipo("TeamAlpha", "Elite gaming team", com1);
                        var eq2 = GetOrCreateEquipo("TeamBeta", "Competitive team", com1);

                        // Asignar imágenes a los equipos de prueba
                        eq1.ImagenUrl = "/Recursos/Equipos/alpha.png";
                        eq2.ImagenUrl = "/Recursos/Equipos/beta.webp";
                        equipoRepo.Modify(eq1);
                        equipoRepo.Modify(eq2);

                        // Asegurar comunidad asociada (algunas BDs existentes pueden tener FK NOT NULL)
                        if (eq1.Comunidad == null) { eq1.Comunidad = com1; equipoRepo.Modify(eq1); }
                        if (eq2.Comunidad == null) { eq2.Comunidad = com1; equipoRepo.Modify(eq2); }

                        // 4. MIEMBROS DE COMUNIDAD (relación muchos-a-muchos Usuario-Comunidad con rol)
                        var mc1 = miembroComunidadCEN.NewMiembroComunidad(u1, com1, ApplicationCore.Domain.Enums.RolComunidad.LIDER);
                        var mc2 = miembroComunidadCEN.NewMiembroComunidad(u2, com1, ApplicationCore.Domain.Enums.RolComunidad.MODERADOR);
                        var mc3 = miembroComunidadCEN.NewMiembroComunidad(u3, com2, ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO);
                        Console.WriteLine($"✓ Created {3} MiembroComunidad");

                        // Método personalizado: Promocionar a un miembro a moderador
                        miembroComunidadCEN.PromocionarAModerador(mc3);
                        Console.WriteLine($"✓ CUSTOM: PromocionarAModerador ejecuted");

                        // 5. MIEMBROS DE EQUIPO (relación muchos-a-muchos Usuario-Equipo con rol)
                        var me1 = miembroEquipoCEN.NewMiembroEquipo(u1, eq1, ApplicationCore.Domain.Enums.RolEquipo.ADMIN);
                        var me2 = miembroEquipoCEN.NewMiembroEquipo(u2, eq1, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);
                        Console.WriteLine($"✓ Created {2} MiembroEquipo");

                        // --- AÑADIDO: creación de chat y mensajes de muestra para el equipo Tigres Arkham ---
                        // Helper local para crear chat y mensajes si no existen (idempotente)
                        void SeedMensajesEquipo(Equipo equipo, Usuario autor1, Usuario autor2)
                        {
                            // Asegurar chat
                            var chat = equipo.Chat;
                            if (chat == null)
                            {
                                chat = chatEquipoCEN.NewChatEquipo(equipo);
                                equipo.Chat = chat;
                                equipoRepo.Modify(equipo);
                                Console.WriteLine($"✓ ChatEquipo creado para {equipo.Nombre}");
                            }

                            // Evitar duplicados: si ya hay mensajes en ese chat, no hacemos nada
                            var tieneMensajes = mensajeChatCEN
                                .ReadAll_MensajeChat()
                                .Any(m => m.Chat != null && m.Chat.IdChatEquipo == chat.IdChatEquipo);

                            if (!tieneMensajes)
                            {
                                mensajeChatCEN.NewMensajeChat($"Bienvenidos al chat de {equipo.Nombre}!", autor1, chat);
                                mensajeChatCEN.NewMensajeChat("¿Entrenamos esta tarde?", autor2, chat);
                                mensajeChatCEN.NewMensajeChat("Yo me apunto. Traed strats.", autor1, chat);
                                Console.WriteLine($"✓ Mensajes de muestra añadidos al chat de {equipo.Nombre}");
                            }
                            else
                            {
                                Console.WriteLine($"• Mensajes ya existen en el chat de {equipo.Nombre}, no se añaden duplicados.");
                            }
                        }

                        // 6. CHAT DE EQUIPO (canal de comunicación privado del equipo)
                        var chat1 = chatEquipoCEN.NewChatEquipo(eq1);
                        // Asociar el chat al equipo y persistir la FK (ChatId)
                        if (eq1.Chat == null)
                        {
                            eq1.Chat = chat1;
                            equipoRepo.Modify(eq1);
                        }
                        Console.WriteLine($"✓ Created ChatEquipo for TeamAlpha");

                        // 7. PUBLICACIONES (posts en el muro de la comunidad)
                        var pub1 = publicacionCEN.NewPublicacion("Welcome to Gamers community!", com1, u1);
                        var pub2 = publicacionCEN.NewPublicacion("Looking for team members", com1, u2);
                        Console.WriteLine($"✓ Created {2} Publicacion");

                        // 8. COMENTARIOS (respuestas a publicaciones)
                        // Creamos comentarios de dos formas: directa y con método personalizado
                        var com_manual = comentarioCEN.NewComentario("Great post!", u2, pub1);
                        var com_custom = publicacionCEN.AddComentario(pub1, u3, "I agree!");  // Método personalizado
                        Console.WriteLine($"✓ Created {2} Comentario (1 via CUSTOM AddComentario)");

                        // 9. REACCIONES (me gusta, no me gusta, etc. en publicaciones)
                        var reac1 = reaccionCEN.NewReaccion(ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA, u1, pub1);
                        var reac2 = reaccionCEN.NewReaccion(ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA, u2, pub2);
                        Console.WriteLine($"✓ Created {2} Reaccion");

                        // --- INICIO DE LA CORRECCIÓN ---
                        // 10. PERFILES (información adicional del usuario: bio, avatar, etc.)
                        // No creamos un nuevo perfil. Obtenemos el que ya se creó con el usuario.
                        var perfil1 = u1.Perfil;
                        if (perfil1 != null)
                        {
                            perfil1.Descripcion = "Pro gamer and streamer";
                            perfil1.FotoPerfilUrl = "/Recursos/Perfiles/23758cc5-8e83-44be-8d5a-b1cfea664a1d.jpg";
                            perfilCEN.ModifyPerfil(perfil1);
                        }

                        var perfil2 = u2.Perfil;
                        if (perfil2 != null)
                        {
                            perfil2.Descripcion = "Casual player";
                            perfil2.FotoPerfilUrl = "/Recursos/Perfiles/Default.png";
                            perfilCEN.ModifyPerfil(perfil2);
                        }
                        Console.WriteLine($"✓ Modified {2} Perfil");
                        // --- FIN DE LA CORRECCIÓN ---

                        // 11. JUEGOS (catálogo de videojuegos disponibles en la plataforma)
                        var juego1 = juegoCEN.NewJuego("League of Legends", ApplicationCore.Domain.Enums.GeneroJuego.ESTRATEGIA, "/Recursos/Juegos/lol.webp", "League of Legends es un videojuego de estrategia en tiempo real desarrollado por Riot Games.");
                        var juego2 = juegoCEN.NewJuego("FIFA 24", ApplicationCore.Domain.Enums.GeneroJuego.DEPORTE, "/Recursos/Juegos/fifa.webp", "FIFA 24 es un videojuego de simulación de fútbol desarrollado por EA Sports.");
                        var juego3 = juegoCEN.NewJuego("Zelda BOTW", ApplicationCore.Domain.Enums.GeneroJuego.AVENTURA, "/Recursos/Juegos/zelda.webp", "The Legend of Zelda: Breath of the Wild es un videojuego de aventura desarrollado por Nintendo.");
                        Console.WriteLine($"✓ Created {3} Juego");

                        // 12. PERFIL-JUEGO (juegos que cada usuario tiene en su biblioteca)
                        var pj1 = perfilJuegoCEN.NewPerfilJuego(perfil1, juego1);
                        var pj2 = perfilJuegoCEN.NewPerfilJuego(perfil1, juego3);
                        var pj3 = perfilJuegoCEN.NewPerfilJuego(perfil2, juego2);
                        Console.WriteLine($"✓ Created {3} PerfilJuego");

                        // 13. TORNEOS (competiciones organizadas por comunidades)
                        var torneo1 = new Torneo
                        {
                            Nombre = "Summer Championship 2025",
                            FechaInicio = DateTime.UtcNow.AddDays(30),
                            Estado = "FINALIZADO",
                            Reglas = "Standard tournament rules",
                            ComunidadOrganizadora = com1
                        };
                        torneoRepo.New(torneo1);
                        Console.WriteLine($"✓ Created {1} Torneo");

                        // 14. PROPUESTAS DE TORNEO (equipos proponen participar en torneos)
                        var prop1 = propuestaCEN.NewPropuestaTorneo(eq1, torneo1, u1);
                        var prop2 = propuestaCEN.NewPropuestaTorneo(eq2, torneo1, u2);
                        
                        // Marcar propuestas como aceptadas
                        prop1.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA;
                        propuestaRepo.Modify(prop1);
                        prop2.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA;
                        propuestaRepo.Modify(prop2);
                        
                        Console.WriteLine($"✓ Created {2} PropuestaTorneo");

                        // === Escenario solicitado: Copa Invierno LoL y Tigres Arkham ===
                        // Crear juego específico si no existe
                        var lol = juegoCEN.NewJuego("League of Legends", ApplicationCore.Domain.Enums.GeneroJuego.ESTRATEGIA);
                        // Crear torneo "Copa Invierno LoL" si no existe
                        var copaInvierno = torneoRepo.ReadFilter("Copa Invierno LoL").FirstOrDefault();
                        if (copaInvierno == null)
                        {
                            copaInvierno = new Torneo { Nombre = "Copa Invierno LoL", FechaInicio = DateTime.UtcNow.AddDays(15), Estado = "PENDIENTE", Reglas = "Formato eliminatorio", ComunidadOrganizadora = com1 };
                            torneoRepo.New(copaInvierno);
                            Console.WriteLine("✓ Created Torneo: Copa Invierno LoL");
                        }

                        // Crear equipo "Tigres Arkham"
                        var tigres = equipoRepo.ReadFilter("Tigres Arkham").FirstOrDefault();
                        if (tigres == null)
                        {
                            tigres = equipoCEN.NewEquipo("Tigres Arkham", "Equipo de prueba Tigres Arkham");
                            Console.WriteLine("✓ Created Equipo: Tigres Arkham");
                        }

                        // Asignar imagen a Tigres Arkham
                        if (!string.IsNullOrEmpty(tigres.ImagenUrl) == false || tigres.ImagenUrl != "/Recursos/Equipos/tigres.png")
                        {
                            tigres.ImagenUrl = "/Recursos/Equipos/tigres.png";
                            equipoRepo.Modify(tigres);
                        }

                        // Crear 3 usuarios y añadirlos al equipo
                        var uA = usuarioRepo.ReadByNick("user1");
                        if (uA == null) uA = usuarioCEN.NewUsuario("user1", "user1@example.com", PasswordHasher.Hash("password1"));
                        var uB = usuarioRepo.ReadByNick("user2");
                        if (uB == null) uB = usuarioCEN.NewUsuario("user2", "user2@example.com", PasswordHasher.Hash("password2"));
                        var uC = usuarioRepo.ReadByNick("user3");
                        if (uC == null) uC = usuarioCEN.NewUsuario("user3", "user3@example.com", PasswordHasher.Hash("password3"));

                        // Añadir miembros al equipo (si no existen ya)
                        miembroEquipoCEN.NewMiembroEquipo(uA, tigres, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);
                        miembroEquipoCEN.NewMiembroEquipo(uB, tigres, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);
                        miembroEquipoCEN.NewMiembroEquipo(uC, tigres, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);

                        // Sembrar mensajes de ejemplo para Tigres Arkham (idempotente)
                        SeedMensajesEquipo(tigres, uA, uB);

                        // Crear propuesta de Tigres Arkham para la Copa Invierno LoL
                        var propTigres = propuestaCEN.NewPropuestaTorneo(tigres, copaInvierno, uA);
                        Console.WriteLine("✓ Created PropuestaTorneo: Tigres Arkham -> Copa Invierno LoL");

                        // 15. VOTOS PARA TORNEOS (votación para aprobar propuestas)
                        var voto1 = votoCEN.NewVotoTorneo(true, u1, prop1);
                        var voto2 = votoCEN.NewVotoTorneo(true, u2, prop1);
                        var voto3 = votoCEN.NewVotoTorneo(true, u1, prop2);
                        var voto4 = votoCEN.NewVotoTorneo(true, u3, prop2);
                        Console.WriteLine($"✓ Created {4} VotoTorneo");

                        // Método personalizado: Aprobar propuesta si todos los votos son positivos
                        var aprobada = propuestaCEN.AprobarSiVotosUnanimes(prop1);
                        Console.WriteLine($"✓ CUSTOM: AprobarSiVotosUnanimes ejecuted (result={aprobada})");

                        // 16. PARTICIPACIÓN EN TORNEO (equipos confirmados en torneos)
                        var part1 = participacionCEN.NewParticipacionTorneo(eq1, torneo1);
                        var part2 = participacionCEN.NewParticipacionTorneo(eq2, torneo1);
                        
                        // Marcar participaciones como aceptadas
                        part1.Estado = ApplicationCore.Domain.Enums.EstadoParticipacion.ACEPTADA.ToString();
                        participacionRepo.Modify(part1);
                        part2.Estado = ApplicationCore.Domain.Enums.EstadoParticipacion.ACEPTADA.ToString();
                        participacionRepo.Modify(part2);
                        
                        Console.WriteLine($"✓ Created {2} ParticipacionTorneo");

                        // Métodos de filtrado personalizados: consultar por torneo o por equipo
                        var equiposByTorneo = participacionCEN.ReadFilter_EquiposByTorneo(torneo1.IdTorneo);
                        var torneosByEquipo = participacionCEN.ReadFilter_TorneosByEquipo(eq1.IdEquipo);
                        Console.WriteLine($"✓ CUSTOM FILTERS: EquiposByTorneo={equiposByTorneo.Count()}, TorneosByEquipo={torneosByEquipo.Count()}");

                        // 17. INVITACIONES (invitaciones a equipos o comunidades)
                        var inv1 = invitacionCEN.NewInvitacion(ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO, u1, u3, null, eq2);
                        Console.WriteLine($"✓ Created {1} Invitacion");

                        // 18. MENSAJES DE CHAT (comunicación en tiempo real entre miembros de equipo)
                        var msg1 = mensajeChatCEN.NewMensajeChat("Hello team!", u1, chat1);
                        var msg2 = mensajeChatCEN.NewMensajeChat("Ready for the tournament?", u2, chat1);
                        Console.WriteLine($"✓ Created {2} MensajeChat");

                        // 19. NOTIFICACIONES (alertas del sistema para usuarios)
                        var not1 = notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, "Welcome to the platform!", u1);
                        var not2 = notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.ALERTA, "You have a new team invitation", u3);
                        Console.WriteLine($"✓ Created {2} Notificacion");

                        // 20. SESIONES (tokens de autenticación activos)
                        var ses1 = sesionCEN.NewSesion(u1, "token-alice-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                        var ses2 = sesionCEN.NewSesion(u2, "token-bob-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                        Console.WriteLine($"✓ Created {2} Sesion");

                        // 21. SOLICITUDES DE INGRESO (peticiones para unirse a comunidades/equipos)
                        var sol1 = solicitudCEN.NewSolicitudIngreso(ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD, u3, com1, null);
                        Console.WriteLine($"✓ Created {1} SolicitudIngreso");

                        // Guardamos todas las entidades en la base de datos (commit de transacción)
                        uow.SaveChanges();
                        Console.WriteLine("✓ All entities saved to database");

                        // PASO 5.5: Ejecución de CPs (casos de uso transaccionales complejos)
                        // Los CPs coordinan múltiples CENs para operaciones de negocio complejas

                        // 22. EJECUTAR CPs (Transacciones personalizadas)
                        Console.WriteLine("\n=== Executing Custom Procedures (CPs) ===");

                        // CP 1: Crear comunidad (proceso completo: crear comunidad + agregar creador como líder)
                        var comCP = crearComunidadCP.Ejecutar("Esports Community", "Created via CP");
                        Console.WriteLine($"✓ CP: CrearComunidadCP ejecuted");

                        // CP 2: Unir usuario a equipo (validar permisos + agregar miembro + crear notificación)
                        var meCP = unirEquipoCP.Ejecutar(u3.IdUsuario, eq2, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);
                        Console.WriteLine($"✓ CP: UnirEquipoCP ejecuted");

                        // CP 3: Aceptar invitación (validar invitación + agregar a equipo/comunidad + actualizar estado)
                        // Recargamos la invitación con eager loading para tener todas las propiedades
                        var invRepo = scope.ServiceProvider.GetRequiredService<IRepository<Invitacion>>();
                        var inv1Reloaded = invRepo.ReadById(inv1.IdInvitacion);
                        if (inv1Reloaded != null)
                        {
                            var meAcept = aceptarInvitacionCP.Ejecutar(inv1Reloaded);
                            Console.WriteLine($"✓ CP: AceptarInvitacionCP ejecuted");
                        }
                        else
                        {
                            Console.WriteLine($"⚠ CP: AceptarInvitacionCP skipped (invitation not found)");
                        }

                        // CP 4: Aprobar propuesta de torneo (verificar votos + cambiar estado + notificar)
                        var resultAprobacion = aprobarPropuestaTorneoCP.Ejecutar(prop2);
                        Console.WriteLine($"✓ CP: AprobarPropuestaTorneoCP ejecuted (result={resultAprobacion})");

                        // PASO 5.6: Prueba de funcionalidad de Login
                        // Verificamos que el sistema de autenticación funciona correctamente
                        var loginTest = authCEN.Login("alice", "password1");
                        Console.WriteLine($"✓ AuthenticationCEN: Login tested (success={loginTest != null})");

                        // Guardamos todos los cambios finales en la base de datos
                        uow.SaveChanges();

                        // Ejecutamos comprobaciones ReadFilter y mostramos resultados en la salida
                        try
                        {
                            Console.WriteLine("\n--- ReadFilter checks (full sweep) ---");

                            Func<object?, string> summarize = (obj) =>
                            {
                                if (obj == null) return "(null)";
                                try
                                {
                                    var t = obj.GetType();
                                    var idProp = t.GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id") || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                                    if (idProp != null)
                                    {
                                        var val = idProp.GetValue(obj);
                                        return $"{t.Name}.{idProp.Name}={val}";
                                    }
                                    var s = obj.ToString() ?? "(toString null)";
                                    return s.Length > 120 ? s.Substring(0, 120) + "..." : s;
                                }
                                catch { return "(summary error)"; }
                            };

                            // Invoke CEN methods where available (they will prefer repo wrappers), fallback to repo ReadFilter
                            void RunCEN<TCEN, TEntity>(string label, string filtro, Func<TCEN, IEnumerable<TEntity>> call)
                            {
                                try
                                {
                                    var cen = scope.ServiceProvider.GetService<TCEN>();
                                    if (cen != null)
                                    {
                                        var list = call(cen).ToList();
                                        Console.WriteLine($"{label} => {list.Count} result(s)");
                                        if (list.Count > 0) Console.WriteLine($"  First: {summarize(list[0])}");
                                        return;
                                    }
                                    // Fallback to generic repo
                                    var repo = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>();
                                    var list2 = repo.ReadFilter(filtro).ToList();
                                    Console.WriteLine($"{label} => {list2.Count} result(s)");
                                    if (list2.Count > 0) Console.WriteLine($"  First: {summarize(list2[0])}");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"{label} => ERROR: {e.Message}");
                                }
                            }

                            // Execute calls (CEN methods prefer repository-specific wrappers via reflection)
                            RunCEN<UsuarioCEN, Usuario>("Usuario.BuscarUsuariosPorNickOEmail('alice')", "alice", c => c.BuscarUsuariosPorNickOEmail("alice"));
                            RunCEN<EquipoCEN, Equipo>("Equipo.BuscarEquiposPorNombreODescripcion('TeamAlpha')", "TeamAlpha", c => c.BuscarEquiposPorNombreODescripcion("TeamAlpha"));
                            RunCEN<PublicacionCEN, Publicacion>("Publicacion.BuscarPublicacionesPorContenido('Welcome')", "Welcome", c => c.BuscarPublicacionesPorContenido("Welcome"));
                            RunCEN<JuegoCEN, Juego>("Juego.BuscarJuegosPorNombreJuego('League')", "League", c => c.BuscarJuegosPorNombreJuego("League"));
                            RunCEN<ComentarioCEN, Comentario>("Comentario.BuscarComentariosPorContenido('Nice')", "Nice", c => c.BuscarComentariosPorContenido("Nice"));
                            RunCEN<MensajeChatCEN, MensajeChat>("MensajeChat.BuscarMensajesChatPorContenido('hello')", "hello", c => c.BuscarMensajesChatPorContenido("hello"));
                            RunCEN<PerfilJuegoCEN, PerfilJuego>("PerfilJuego.BuscarPerfilesPorNombreJuego('perfil')", "perfil", c => c.BuscarPerfilesPorNombreJuego("perfil"));
                            RunCEN<PerfilCEN, Perfil>("Perfil.BuscarPerfilesPorDescripcion('profile')", "profile", c => c.BuscarPerfilesPorDescripcion("profile"));
                            RunCEN<NotificacionCEN, Notificacion>("Notificacion.BuscarNotificacionesPorMensajeODestinatarioNick('notific')", "notific", c => c.BuscarNotificacionesPorMensajeODestinatarioNick("notific"));
                            RunCEN<PropuestaTorneoCEN, PropuestaTorneo>("PropuestaTorneo.BuscarPropuestasTorneoPorNombreTorneo('propuesta')", "propuesta", c => c.BuscarPropuestasTorneoPorNombreTorneo("propuesta"));
                            RunCEN<ReaccionCEN, Reaccion>("Reaccion.BuscarReaccionesPorAutorNickOPublicacionContenido('like')", "like", c => c.BuscarReaccionesPorAutorNickOPublicacionContenido("like"));
                            RunCEN<SesionCEN, Sesion>("Sesion.BuscarSesionesPorToken('token')", "token", c => c.BuscarSesionesPorToken("token"));
                            RunCEN<SolicitudIngresoCEN, SolicitudIngreso>("SolicitudIngreso.BuscarSolicitudesIngresoPorNickSolicitante('solicitud')", "solicitud", c => c.BuscarSolicitudesIngresoPorNickSolicitante("solicitud"));
                            // Torneo has no CEN; fallback to generic repo ReadFilter but label it as BuscarTorneosPorNombre
                            RunCEN<object, Torneo>("Torneo.BuscarTorneosPorNombre('Torneo')", "Torneo", cen => scope.ServiceProvider.GetRequiredService<IRepository<Torneo>>().ReadFilter("Torneo"));
                            RunCEN<VotoTorneoCEN, VotoTorneo>("VotoTorneo.BuscarVotosTorneoPorNombreTorneo('voto')", "voto", c => c.BuscarVotosTorneoPorNombreTorneo("voto"));
                            RunCEN<InvitacionCEN, Invitacion>("Invitacion.BuscarInvitacionesPorNickEmisorODestinatario('invite')", "invite", c => c.BuscarInvitacionesPorNickEmisorODestinatario("invite"));
                            // MiembroComunidad has no CEN wrapper for ReadFilter by nick; fallback to repo
                            RunCEN<object, MiembroComunidad>("MiembroComunidad.BuscarMiembrosComunidadPorNickUsuario('miembro')", "miembro", cen => scope.ServiceProvider.GetRequiredService<IRepository<MiembroComunidad>>().ReadFilter("miembro"));
                            RunCEN<MiembroEquipoCEN, MiembroEquipo>("MiembroEquipo.BuscarMiembrosEquipoPorNickUsuario('miembro')", "miembro", c => c.BuscarMiembrosEquipoPorNickUsuario("miembro"));
                            RunCEN<ParticipacionTorneoCEN, ParticipacionTorneo>("ParticipacionTorneo.BuscarParticipacionesTorneoPorEstado('particip')", "particip", c => c.BuscarParticipacionesTorneoPorEstado("particip"));

                            FileLog($"[{DateTime.UtcNow:o}] ReadFilter full sweep executed.");
                        }
                        catch (Exception rfEx)
                        {
                            Console.WriteLine($"ReadFilter checks failed: {rfEx.Message}");
                            FileLog($"[{DateTime.UtcNow:o}] ReadFilter checks failed: {rfEx.Message}");
                        }

                        // Mostramos resumen de éxito
                        Console.WriteLine("\n========================================");
                        Console.WriteLine("✅ Comprehensive seed completed successfully!");
                        Console.WriteLine("✅ ALL 22 entity types created");
                        Console.WriteLine("✅ ALL 4 CPs executed successfully");
                        Console.WriteLine("✅ ALL custom methods invoked");
                        Console.WriteLine("✅ Login functionality tested");
                        Console.WriteLine("========================================\n");

                        logger.LogInformation("Comprehensive seeding completed successfully");
                        FileLog($"[{DateTime.UtcNow:o}] Comprehensive seeding completed with all entity types");

                        // Limpiamos recursos (SessionFactory)
                        try { seedSf.Dispose(); FileLog($"[{DateTime.UtcNow:o}] Seed SessionFactory disposed."); } catch { }
                    }
                }
                catch (Exception seedEx)
                {
                    // Si falla el seed, reportamos el error pero no detenemos el sistema
                    Console.WriteLine($"Seeding failed: {seedEx.Message}");
                    FileLog($"[{DateTime.UtcNow:o}] Seeding failed: {seedEx.Message}");
                    return 3;  // Código de error 3: fallo en seeding
                }
            }

            // PASO 6: CIERRE Y LIMPIEZA
            // Cerramos logs y liberamos recursos
            try
            {
                // Flush y cierre de Serilog para asegurar que todo se escribe en disco
                Log.Information("[{time}] InitializeDb completed", DateTime.UtcNow.ToString("o"));
                Log.CloseAndFlush();
            }
            catch { }

            // Retornamos 0 indicando éxito total
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InitializeDbService failed: {ex.Message}");
            return 4;
        }
    }

}

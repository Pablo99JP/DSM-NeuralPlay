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
using ApplicationCore.Infrastructure.Memory;
using Infrastructure.NHibernate;
using Microsoft.Data.SqlClient;

public static class InitializeDbService
{
    // Returns 0 on success, non-zero on failure
    public static async Task<int> RunAsync(string[] args, TextWriter? externalLogWriter = null)
    {
        try
        {
            // Parse args similarly to previous Program.cs
            string mode = "inmemory";
            bool forceDrop = false;
            bool confirm = false;
            string dbName = "ProjectDatabase";
            bool doSeed = false;
            string? dataDirArg = null;
            bool verbose = false;
            string? logFile = null;
            string? targetConnection = null;
            string? targetDialect = null;
            foreach (var a in args)
            {
                if (a.StartsWith("--mode=", StringComparison.OrdinalIgnoreCase))
                {
                    mode = a.Substring("--mode=".Length).ToLowerInvariant();
                }
                else if (a.Equals("--force-drop", StringComparison.OrdinalIgnoreCase))
                {
                    forceDrop = true;
                }
                else if (a.Equals("--confirm", StringComparison.OrdinalIgnoreCase))
                {
                    confirm = true;
                }
                else if (a.StartsWith("--db-name=", StringComparison.OrdinalIgnoreCase))
                {
                    dbName = a.Substring("--db-name=".Length);
                    if (string.IsNullOrWhiteSpace(dbName)) dbName = "ProjectDatabase";
                }
                else if (a.Equals("--seed", StringComparison.OrdinalIgnoreCase))
                {
                    doSeed = true;
                }
                else if (a.StartsWith("--data-dir=", StringComparison.OrdinalIgnoreCase))
                {
                    dataDirArg = a.Substring("--data-dir=".Length);
                    if (string.IsNullOrWhiteSpace(dataDirArg)) dataDirArg = null;
                }
                else if (a.StartsWith("--target-connection=", StringComparison.OrdinalIgnoreCase))
                {
                    targetConnection = a.Substring("--target-connection=".Length);
                    if (string.IsNullOrWhiteSpace(targetConnection)) targetConnection = null;
                }
                else if (a.StartsWith("--dialect=", StringComparison.OrdinalIgnoreCase))
                {
                    targetDialect = a.Substring("--dialect=".Length);
                    if (string.IsNullOrWhiteSpace(targetDialect)) targetDialect = null;
                }
                else if (a.Equals("--verbose", StringComparison.OrdinalIgnoreCase) || a.Equals("-v", StringComparison.OrdinalIgnoreCase))
                {
                    verbose = true;
                }
                else if (a.StartsWith("--log-file=", StringComparison.OrdinalIgnoreCase))
                {
                    logFile = a.Substring("--log-file=".Length);
                    if (string.IsNullOrWhiteSpace(logFile)) logFile = null;
                }
            }

            if (mode == "schemaexport")
            {
                // Configure Serilog via centralized configurator (Infrastructure.Logging)
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
                            if (tries >= attempts)
                            {
                                throw;
                            }
                            // small backoff
                            try { await Task.Delay(delayMs); } catch { }
                        }
                    }
                }

                try
                {
                    Infrastructure.Logging.SerilogConfigurator.Configure(logFile, verbose);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to configure Serilog via SerilogConfigurator: {ex.Message}");
                    // Fallback to a minimal console logger
                    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
                }
                using var loggerFactory = new SerilogLoggerFactory(Log.Logger, dispose: false);
                var logger = loggerFactory.CreateLogger("InitializeDb");

                // FileLog writes into the optional externalLogWriter (used by tests). Serilog handles console/file sinks.
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
                    catch { }
                }

                // Resolve data directory
                string dataDir;
                if (!string.IsNullOrWhiteSpace(dataDirArg))
                {
                    dataDir = Path.GetFullPath(dataDirArg);
                }
                else
                {
                    var repoData = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data"));
                    dataDir = repoData;
                }
                Directory.CreateDirectory(dataDir);

                Console.WriteLine($"InitializeDb Run: mode={mode} logFile={logFile} externalLogWriterIsNull={externalLogWriter==null} verbose={verbose}");
                if (externalLogWriter != null)
                {
                    FileLog($"[{DateTime.UtcNow:o}] InitializeDb log started (external writer)");
                }

                logger.LogInformation("InitializeDb - running NHibernate SchemaExport mode...");
                FileLog($"[{DateTime.UtcNow:o}] InitializeDb started in schemaexport mode. dbName={dbName} verbose={verbose}");

                var mdfPath = Path.Combine(dataDir, dbName + ".mdf");
                string? lastConnectionString = null;
                string? lastDialect = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(targetConnection))
                    {
                        // Use the provided connection string and dialect directly instead of attempting LocalDB
                        var dialectToUse = string.IsNullOrWhiteSpace(targetDialect) ? "NHibernate.Dialect.MsSql2012Dialect" : targetDialect;
                        FileLog($"[{DateTime.UtcNow:o}] Exporting schema to target connection using connection: {targetConnection} dialect: {dialectToUse}");
                        Console.WriteLine($"Exporting schema to target connection: {targetConnection} dialect: {dialectToUse}");
                        // If targetting SQL Server, prefer to generate SQL script and run it via sqlcmd to avoid driver binding issues.
                        var driver = dialectToUse.Contains("MsSql", StringComparison.OrdinalIgnoreCase) ? "NHibernate.Driver.SqlClientDriver" : null;
                        var cfgForExport = NHibernateHelper.BuildConfiguration();
                        if (!string.IsNullOrWhiteSpace(dialectToUse)) cfgForExport.SetProperty("dialect", dialectToUse);
                        if (!string.IsNullOrWhiteSpace(driver)) cfgForExport.SetProperty("connection.driver_class", driver);
                        cfgForExport.SetProperty("connection.connection_string", targetConnection);

                        var schemaFile = Path.Combine(dataDir, dbName + "_schema.sql");
                        try
                        {
                            // Write DDL SQL to a file
                            var export = new NHibernate.Tool.hbm2ddl.SchemaExport(cfgForExport);
                            export.SetOutputFile(schemaFile);
                            export.Create(false, false);

                            // Parse target connection to get server and database for sqlcmd
                            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(targetConnection);
                            var server = builder.DataSource;
                            var database = builder.InitialCatalog ?? dbName;

                            // Execute the generated SQL script using sqlcmd (Windows)
                            var psi = new System.Diagnostics.ProcessStartInfo("sqlcmd", $"-S \"{server}\" -d \"{database}\" -E -i \"{schemaFile}\"") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
                            using var proc = System.Diagnostics.Process.Start(psi);
                            if (proc != null)
                            {
                                var outText = proc.StandardOutput.ReadToEnd();
                                var errText = proc.StandardError.ReadToEnd();
                                proc.WaitForExit(60000);
                                if (proc.ExitCode != 0)
                                {
                                    Console.WriteLine($"sqlcmd failed: {errText}");
                                    throw new InvalidOperationException($"sqlcmd failed: {errText}");
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException("Failed to start sqlcmd process");
                            }

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
                        lastConnectionString = targetConnection;
                        lastDialect = dialectToUse;
                        logger.LogInformation("SchemaExport to target connection completed.");
                        FileLog($"[{DateTime.UtcNow:o}] SchemaExport to target connection completed successfully.");
                    }
                    else
                    {
                        logger.LogInformation($"Attempting SchemaExport to LocalDB ({Path.GetFileName(mdfPath)})...");
                        FileLog($"[{DateTime.UtcNow:o}] Attempting SchemaExport to LocalDB ({mdfPath})");

                        if (File.Exists(mdfPath) && !forceDrop)
                        {
                            logger.LogWarning("LocalDB MDF already exists at {mdfPath} and --force-drop not provided. Skipping LocalDB attempt and falling back to SQLite.", mdfPath);
                            FileLog($"[{DateTime.UtcNow:o}] MDF exists and --force-drop not set. Skipping LocalDB.");
                            throw new InvalidOperationException("LocalDB MDF exists and force-drop not set.");
                        }

                        if (forceDrop && !confirm)
                        {
                            Console.WriteLine("--force-drop specified but --confirm not provided. Aborting destructive action. Falling back to SQLite.");
                            FileLog($"[{DateTime.UtcNow:o}] --force-drop without --confirm; aborting LocalDB destructive action.");
                            throw new InvalidOperationException("Force drop requested without confirm.");
                        }

                        using var masterConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=5;");
                        try
                        {
                            masterConn.Open();

                            if (forceDrop && confirm && File.Exists(mdfPath))
                            {
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

                                try
                                {
                                    Console.WriteLine($"Deleting existing MDF {mdfPath} as --force-drop and --confirm were provided...");
                                    await RetryAsync(() => { File.Delete(mdfPath); return Task.CompletedTask; }, attempts: 3, delayMs: 200);
                                    var logPath = Path.Combine(dataDir, dbName + "_log.ldf");
                                    if (File.Exists(logPath)) await RetryAsync(() => { File.Delete(logPath); return Task.CompletedTask; }, attempts: 3, delayMs: 200);
                                    Console.WriteLine("Existing MDF removed.");
                                }
                                catch (Exception delEx)
                                {
                                    Console.WriteLine($"Failed to delete existing MDF: {delEx.Message}. Falling back to SQLite.");
                                    FileLog($"[{DateTime.UtcNow:o}] Failed to delete existing MDF: {delEx.Message}");
                                    throw;
                                }
                            }

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

                            var dbConn = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Connect Timeout=30;";
                            FileLog($"[{DateTime.UtcNow:o}] Exporting schema to LocalDB using connection: {dbConn}");
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
                    Console.WriteLine($"LocalDB SchemaExport failed: {ex.Message}");
                    logger.LogWarning("Falling back to file-based SQLite SchemaExport...");
                    FileLog($"[{DateTime.UtcNow:o}] LocalDB SchemaExport failed: {ex.Message}. Falling back to SQLite.");
                    var sqlitePath = Path.Combine(dataDir, "project.db");
                    var sqliteConn = $"Data Source={sqlitePath};Version=3;";
                        try
                        {
                            FileLog($"[{DateTime.UtcNow:o}] Exporting schema to SQLite file at {sqlitePath}");
                            await RetryAsync(() => { NHibernateHelper.ExportSchema(sqliteConn, "NHibernate.Dialect.SQLiteDialect"); return Task.CompletedTask; }, attempts: 3, delayMs: 200);
                            lastConnectionString = sqliteConn;
                            lastDialect = "NHibernate.Dialect.SQLiteDialect";
                            logger.LogInformation("SchemaExport to SQLite file completed. Path={path}", sqlitePath);
                            FileLog($"[{DateTime.UtcNow:o}] SchemaExport to SQLite completed successfully. Path={sqlitePath}");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"SQLite SchemaExport also failed: {ex2.Message}");
                            FileLog($"[{DateTime.UtcNow:o}] SQLite SchemaExport failed: {ex2.Message}");
                            Console.WriteLine("InitializeDb schema export failed. Review NHibernate configuration and environment (LocalDB availability, file permissions).");
                            return 2;
                        }
                }

                logger.LogInformation("InitializeDb schema export finished.");
                FileLog($"[{DateTime.UtcNow:o}] InitializeDb schema export finished. connection={lastConnectionString} dialect={lastDialect}");

                if (doSeed)
                {
                    logger.LogInformation("Seeding database via NHibernate repositories (idempotent)...");
                    FileLog($"[{DateTime.UtcNow:o}] Starting idempotent seed. connection={lastConnectionString}");
                    try
                    {
                        if (string.IsNullOrWhiteSpace(lastConnectionString) || string.IsNullOrWhiteSpace(lastDialect))
                        {
                            Console.WriteLine("Warning: connection/dialect for seeding not available. Skipping seed.");
                        }
                        else
                        {
                            var services = new ServiceCollection();
                            var seedCfg = NHibernateHelper.BuildConfiguration();
                            seedCfg.SetProperty("connection.connection_string", lastConnectionString);
                            seedCfg.SetProperty("dialect", lastDialect);
                            if (!string.IsNullOrWhiteSpace(lastDialect) && lastDialect.Contains("MsSql", StringComparison.OrdinalIgnoreCase))
                            {
                                seedCfg.SetProperty("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
                            }
                            var seedSf = seedCfg.BuildSessionFactory();

                            services.AddSingleton(seedSf);
                            services.AddScoped(provider => seedSf.OpenSession());

                            services.AddScoped<IUsuarioRepository, NHibernateUsuarioRepository>();
                            services.AddScoped<IRepository<Comunidad>, NHibernateComunidadRepository>();
                            services.AddScoped<IRepository<Equipo>, NHibernateEquipoRepository>();
                            services.AddScoped<IRepository<MiembroComunidad>, NHibernateMiembroComunidadRepository>();
                            // Register specialized repository interfaces so CENs can depend on them directly
                            services.AddScoped<IParticipacionTorneoRepository, NHibernateParticipacionTorneoRepository>();
                            services.AddScoped<IMiembroEquipoRepository, NHibernateMiembroEquipoRepository>();
                            services.AddScoped<IMiembroComunidadRepository, NHibernateMiembroComunidadRepository>();

                            services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();
                            services.AddScoped<UsuarioCEN>();
                            services.AddScoped<ComunidadCEN>();
                            services.AddScoped<EquipoCEN>();

                            var provider = services.BuildServiceProvider();
                            using var scope = provider.CreateScope();

                            var usuarioCEN = scope.ServiceProvider.GetRequiredService<UsuarioCEN>();
                            var comunidadCEN = scope.ServiceProvider.GetRequiredService<ComunidadCEN>();
                            var equipoCEN = scope.ServiceProvider.GetRequiredService<EquipoCEN>();
                            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                            var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
                            var comunidadRepo = scope.ServiceProvider.GetRequiredService<IRepository<Comunidad>>();
                            var equipoRepo = scope.ServiceProvider.GetRequiredService<IRepository<Equipo>>();
                            var miembroComunidadRepo = scope.ServiceProvider.GetRequiredService<IRepository<MiembroComunidad>>();

                            if (usuarioRepo.ReadByNick("alice") == null)
                            {
                                var u1 = usuarioCEN.NewUsuario("alice", "alice@example.com", PasswordHasher.Hash("password1"));
                                logger.LogInformation("Created user alice (id={id})", u1.IdUsuario);
                                FileLog($"[{DateTime.UtcNow:o}] Created user alice id={u1.IdUsuario}");
                            }
                            if (usuarioRepo.ReadByNick("bob") == null)
                            {
                                var u2 = usuarioCEN.NewUsuario("bob", "bob@example.com", PasswordHasher.Hash("password2"));
                                logger.LogInformation("Created user bob (id={id})", u2.IdUsuario);
                                FileLog($"[{DateTime.UtcNow:o}] Created user bob id={u2.IdUsuario}");
                            }

                            if (!comunidadRepo.ReadFilter("Gamers").Any())
                            {
                                var com = comunidadCEN.NewComunidad("Gamers", "Comunidad de prueba");
                                logger.LogInformation("Created comunidad {id}", com.IdComunidad);
                                FileLog($"[{DateTime.UtcNow:o}] Created comunidad id={com.IdComunidad}");
                            }
                            if (!equipoRepo.ReadFilter("TeamA").Any())
                            {
                                var eq = equipoCEN.NewEquipo("TeamA", "Equipo de ejemplo");
                                logger.LogInformation("Created equipo {id}", eq.IdEquipo);
                                FileLog($"[{DateTime.UtcNow:o}] Created equipo id={eq.IdEquipo}");
                            }

                            var existingCom = comunidadRepo.ReadFilter("Gamers").FirstOrDefault();
                            var existingUser = usuarioRepo.ReadByNick("alice");
                            if (existingCom != null && existingUser != null && !miembroComunidadRepo.ReadFilter(existingUser.Nick ?? "").Any())
                            {
                                var mc = new MiembroComunidad { Usuario = existingUser, Comunidad = existingCom, FechaAlta = DateTime.UtcNow, Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO };
                                miembroComunidadRepo.New(mc);
                                Console.WriteLine($"Created MiembroComunidad for user {existingUser.Nick}");
                                FileLog($"[{DateTime.UtcNow:o}] Created MiembroComunidad user={existingUser.Nick} comunidad={existingCom.Nombre}");
                            }

                            uow.SaveChanges();
                            Console.WriteLine("Seeding completed.");
                            FileLog($"[{DateTime.UtcNow:o}] Seeding completed and changes saved.");

                            try { seedSf.Dispose(); FileLog($"[{DateTime.UtcNow:o}] Seed SessionFactory disposed."); } catch { }
                        }
                    }
                    catch (Exception seedEx)
                    {
                        Console.WriteLine($"Seeding failed: {seedEx.Message}");
                        FileLog($"[{DateTime.UtcNow:o}] Seeding failed: {seedEx.Message}");
                        return 3;
                    }
                }

                try
                {
                    // Flush and close Serilog so file sinks are written to disk
                    Log.Information("[{time}] InitializeDb completed", DateTime.UtcNow.ToString("o"));
                    Log.CloseAndFlush();
                }
                catch { }

                return 0;
            }
            else
            {
                // In-memory validation path (kept for completeness)
                Console.WriteLine("InitializeDb - running in-memory validation...");
                // existing in-memory code simplified
                var usuarioRepo = new InMemoryUsuarioRepository();
                var comunidadRepo = new InMemoryRepository<Comunidad>();
                var equipoRepo = new InMemoryRepository<Equipo>();
                var publicacionRepo = new InMemoryRepository<Publicacion>();
                var comentarioRepo = new InMemoryRepository<Comentario>();
                var reaccionRepo = new InMemoryRepository<Reaccion>();
                var perfilRepo = new InMemoryRepository<Perfil>();
                var juegoRepo = new InMemoryRepository<Juego>();
                var propuestaRepo = new InMemoryRepository<PropuestaTorneo>();
                var participacionRepo = new ApplicationCore.Infrastructure.Memory.InMemoryParticipacionTorneoRepository();
                var miembroComunidadRepo = new ApplicationCore.Infrastructure.Memory.InMemoryMiembroComunidadRepository();
                var miembroEquipoRepo = new ApplicationCore.Infrastructure.Memory.InMemoryMiembroEquipoRepository();
                var notRepo = new InMemoryRepository<Notificacion>();

                var uow = new InMemoryUnitOfWork();
                var usuarioCEN = new UsuarioCEN(usuarioRepo);
                var authCEN = new AuthenticationCEN(usuarioRepo);
                var comunidadCEN = new ComunidadCEN(comunidadRepo);
                var equipoCEN = new EquipoCEN(equipoRepo);
                var publicacionCEN = new PublicacionCEN(publicacionRepo);
                var comentarioCEN = new ComentarioCEN(comentarioRepo);
                var reaccionCEN = new ReaccionCEN(reaccionRepo);
                var perfilCEN = new PerfilCEN(perfilRepo);
                var juegoCEN = new JuegoCEN(juegoRepo);
                var propuestaCEN = new PropuestaTorneoCEN(propuestaRepo);
                var miembroComunidadCEN = new MiembroComunidadCEN(miembroComunidadRepo);

                // small smoke operations
                var u1 = usuarioCEN.NewUsuario("alice", "alice@example.com", PasswordHasher.Hash("password1"));
                var u2 = usuarioCEN.NewUsuario("bob", "bob@example.com", PasswordHasher.Hash("password2"));

                Console.WriteLine($"Created users: {u1.IdUsuario}={u1.Nick}, {u2.IdUsuario}={u2.Nick}");
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InitializeDbService failed: {ex.Message}");
            return 4;
        }
    }
}

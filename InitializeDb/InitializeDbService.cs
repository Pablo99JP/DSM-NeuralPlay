using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
                });
                var logger = loggerFactory.CreateLogger("InitializeDb");

                StreamWriter? logWriter = externalLogWriter != null ? null : null;
                void FileLog(string line)
                {
                    try
                    {
                        if (externalLogWriter != null)
                        {
                            externalLogWriter.WriteLine(line);
                            externalLogWriter.Flush();
                        }
                        else if (logWriter != null)
                        {
                            logWriter.WriteLine(line);
                            logWriter.Flush();
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

                // Open log file if requested and external writer not provided
                if (!string.IsNullOrWhiteSpace(logFile) && externalLogWriter == null)
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(logFile) ?? Path.GetDirectoryName(Path.GetFullPath(logFile)) ?? AppContext.BaseDirectory;
                        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                        logWriter = new StreamWriter(logFile, append: false) { AutoFlush = true };
                        FileLog($"[{DateTime.UtcNow:o}] InitializeDb log started");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Could not open log file {file} for writing: {msg}", logFile, ex.Message);
                    }
                }

                logger.LogInformation("InitializeDb - running NHibernate SchemaExport mode...");
                FileLog($"[{DateTime.UtcNow:o}] InitializeDb started in schemaexport mode. dbName={dbName} verbose={verbose}");

                var mdfPath = Path.Combine(dataDir, dbName + ".mdf");
                string? lastConnectionString = null;
                string? lastDialect = null;
                try
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
                                File.Delete(mdfPath);
                                var logPath = Path.Combine(dataDir, dbName + "_log.ldf");
                                if (File.Exists(logPath)) File.Delete(logPath);
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
                        NHibernateHelper.ExportSchema(dbConn, "NHibernate.Dialect.MsSql2012Dialect");
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
                        NHibernateHelper.ExportSchema(sqliteConn, "NHibernate.Dialect.SQLiteDialect");
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
                            var seedSf = seedCfg.BuildSessionFactory();

                            services.AddSingleton(seedSf);
                            services.AddScoped(provider => seedSf.OpenSession());

                            services.AddScoped<IUsuarioRepository, NHibernateUsuarioRepository>();
                            services.AddScoped<IRepository<Comunidad>, NHibernateComunidadRepository>();
                            services.AddScoped<IRepository<Equipo>, NHibernateEquipoRepository>();
                            services.AddScoped<IRepository<MiembroComunidad>, NHibernateMiembroComunidadRepository>();

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
                    if (logWriter != null)
                    {
                        FileLog($"[{DateTime.UtcNow:o}] InitializeDb completed. Closing log file.");
                        logWriter.Dispose();
                        logWriter = null;
                    }
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
                var participacionRepo = new InMemoryRepository<ParticipacionTorneo>();
                var miembroComunidadRepo = new InMemoryRepository<MiembroComunidad>();
                var miembroEquipoRepo = new InMemoryRepository<MiembroEquipo>();
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

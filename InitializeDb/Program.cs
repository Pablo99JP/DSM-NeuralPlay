using System;
using System.Linq;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Infrastructure.Memory;
using Infrastructure.NHibernate;
using Microsoft.Data.SqlClient;

// Modes: --mode=inmemory (default) | --mode=schemaexport
// Flags: --force-drop (allow destructive recreate), --confirm (required with --force-drop), --db-name=<name>
string mode = "inmemory";
bool forceDrop = false;
bool confirm = false;
string dbName = "ProjectDatabase";
bool doSeed = false;
string? dataDirArg = null;
bool verbose = false;
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
}

if (mode == "schemaexport")
{
	// Create a logger so verbose runs print debug information
	using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
	{
		builder.AddConsole();
		builder.SetMinimumLevel(verbose ? Microsoft.Extensions.Logging.LogLevel.Debug : Microsoft.Extensions.Logging.LogLevel.Information);
	});
	var logger = loggerFactory.CreateLogger("InitializeDb");

	logger.LogInformation("InitializeDb - running NHibernate SchemaExport mode...");
	// Prefer repository-local InitializeDb/Data (relative to the project folder). When running from the build output
	// AppContext.BaseDirectory typically points to bin/.../net8.0, so move up to the project folder and create Data there.
	// Resolve data directory: prefer explicit --data-dir, otherwise repo-local InitializeDb/Data
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

	// Try LocalDB first (recommended by solution.plan.md)
	var mdfPath = Path.Combine(dataDir, dbName + ".mdf");
	var localDbConn = $"Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename={mdfPath};Integrated Security=True;Connect Timeout=30;";
    string? lastConnectionString = null;
    string? lastDialect = null;
	try
	{
	logger.LogInformation($"Attempting SchemaExport to LocalDB ({Path.GetFileName(mdfPath)})...");

		// Safety: if MDF already exists and user didn't pass --force-drop, skip LocalDB attempt
				if (File.Exists(mdfPath) && !forceDrop)
		{
			logger.LogWarning("LocalDB MDF already exists at {mdfPath} and --force-drop not provided. Skipping LocalDB attempt and falling back to SQLite.", mdfPath);
			throw new InvalidOperationException("LocalDB MDF exists and force-drop not set.");
		}

		// If forceDrop is requested, require explicit --confirm to perform destructive action
		if (forceDrop && !confirm)
		{
			Console.WriteLine("--force-drop specified but --confirm not provided. Aborting destructive action. Falling back to SQLite.");
			throw new InvalidOperationException("Force drop requested without confirm.");
		}

		// Try connect to LocalDB master to ensure LocalDB instance is available
		using var masterConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=5;");
		try
		{
			masterConn.Open();

			// If forceDrop+confirm and MDF exists, attempt to drop database if attached and delete files
			if (forceDrop && confirm && File.Exists(mdfPath))
			{
				try
				{
					// If a database with this name exists, drop it first
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
					throw;
				}
			}

			// If MDF does not exist, create a new database file via CREATE DATABASE ... (specifying filename)
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
					throw;
				}
			}

			// Use Initial Catalog to connect to the newly created/attached DB
			var dbConn = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Connect Timeout=30;";
			NHibernateHelper.ExportSchema(dbConn, "NHibernate.Dialect.MsSql2012Dialect");
			lastConnectionString = dbConn;
			lastDialect = "NHibernate.Dialect.MsSql2012Dialect";
			logger.LogInformation("SchemaExport to LocalDB completed.");
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
		var sqlitePath = Path.Combine(dataDir, "project.db");
		var sqliteConn = $"Data Source={sqlitePath};Version=3;";
		try
		{
			NHibernateHelper.ExportSchema(sqliteConn, "NHibernate.Dialect.SQLiteDialect");
			lastConnectionString = sqliteConn;
			lastDialect = "NHibernate.Dialect.SQLiteDialect";
			logger.LogInformation("SchemaExport to SQLite file completed. Path={path}", sqlitePath);
		}
		catch (Exception ex2)
		{
			Console.WriteLine($"SQLite SchemaExport also failed: {ex2.Message}");
			Console.WriteLine("InitializeDb schema export failed. Review NHibernate configuration and environment (LocalDB availability, file permissions).");
			Environment.Exit(1);
		}
	}

	logger.LogInformation("InitializeDb schema export finished.");

	// If requested, run idempotent seed using NHibernate repositories (will persist into the exported DB if supported)
	if (doSeed)
	{
		logger.LogInformation("Seeding database via NHibernate repositories (idempotent)...");
		try
		{
				// Build a session factory configured to the same connection/dialect used for SchemaExport
				if (string.IsNullOrWhiteSpace(lastConnectionString) || string.IsNullOrWhiteSpace(lastDialect))
				{
					Console.WriteLine("Warning: connection/dialect for seeding not available. Skipping seed.");
				}
				else
				{
					// Configure DI so seed logic can resolve repositories, UoW and CENs
					var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

					var seedCfg = NHibernateHelper.BuildConfiguration();
					seedCfg.SetProperty("connection.connection_string", lastConnectionString);
					seedCfg.SetProperty("dialect", lastDialect);
					var seedSf = seedCfg.BuildSessionFactory();

					// Register NHibernate SessionFactory as singleton and ISession as scoped
					services.AddSingleton(seedSf);
					services.AddScoped(provider => seedSf.OpenSession());

					// Register NHibernate-based repositories used by the seed
					services.AddScoped<ApplicationCore.Domain.Repositories.IUsuarioRepository, NHibernateUsuarioRepository>();
					services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<ApplicationCore.Domain.EN.Comunidad>, NHibernateComunidadRepository>();
					services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<ApplicationCore.Domain.EN.Equipo>, NHibernateEquipoRepository>();
					services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<ApplicationCore.Domain.EN.MiembroComunidad>, NHibernateMiembroComunidadRepository>();

					// Register UnitOfWork and CENs
					services.AddScoped<ApplicationCore.Domain.Repositories.IUnitOfWork, NHibernateUnitOfWork>();
					services.AddScoped<ApplicationCore.Domain.CEN.UsuarioCEN>();
					services.AddScoped<ApplicationCore.Domain.CEN.ComunidadCEN>();
					services.AddScoped<ApplicationCore.Domain.CEN.EquipoCEN>();

					var provider = services.BuildServiceProvider();
					using var scope = provider.CreateScope();

					var usuarioCEN = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.CEN.UsuarioCEN>();
					var comunidadCEN = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.CEN.ComunidadCEN>();
					var equipoCEN = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.CEN.EquipoCEN>();
					var uow = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.Repositories.IUnitOfWork>();

					var usuarioRepo = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.Repositories.IUsuarioRepository>();
					var comunidadRepo = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.Repositories.IRepository<ApplicationCore.Domain.EN.Comunidad>>();
					var equipoRepo = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.Repositories.IRepository<ApplicationCore.Domain.EN.Equipo>>();
					var miembroComunidadRepo = scope.ServiceProvider.GetRequiredService<ApplicationCore.Domain.Repositories.IRepository<ApplicationCore.Domain.EN.MiembroComunidad>>();

					// Idempotent user creation
					if (usuarioRepo.ReadByNick("alice") == null)
					{
						var u1 = usuarioCEN.NewUsuario("alice", "alice@example.com", ApplicationCore.Domain.CEN.PasswordHasher.Hash("password1"));
						logger.LogInformation("Created user alice (id={id})", u1.IdUsuario);
					}
					if (usuarioRepo.ReadByNick("bob") == null)
					{
						var u2 = usuarioCEN.NewUsuario("bob", "bob@example.com", ApplicationCore.Domain.CEN.PasswordHasher.Hash("password2"));
						logger.LogInformation("Created user bob (id={id})", u2.IdUsuario);
					}

					// Idempotent comunidad/equipo
					if (!comunidadRepo.ReadFilter("Gamers").Any())
					{
						var com = comunidadCEN.NewComunidad("Gamers", "Comunidad de prueba");
						logger.LogInformation("Created comunidad {id}", com.IdComunidad);
					}
					if (!equipoRepo.ReadFilter("TeamA").Any())
					{
						var eq = equipoCEN.NewEquipo("TeamA", "Equipo de ejemplo");
						logger.LogInformation("Created equipo {id}", eq.IdEquipo);
					}

					// Add a miembro comunidad if not present (using first user/comunidad)
					var existingCom = comunidadRepo.ReadFilter("Gamers").FirstOrDefault();
					var existingUser = usuarioRepo.ReadByNick("alice");
					if (existingCom != null && existingUser != null && !miembroComunidadRepo.ReadFilter(existingUser.Nick ?? "").Any())
					{
						var mc = new ApplicationCore.Domain.EN.MiembroComunidad { Usuario = existingUser, Comunidad = existingCom, FechaAlta = DateTime.UtcNow, Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO };
						miembroComunidadRepo.New(mc);
						Console.WriteLine($"Created MiembroComunidad for user {existingUser.Nick}");
					}

					uow.SaveChanges();
					Console.WriteLine("Seeding completed.");

					try { seedSf.Dispose(); } catch { }
				}
		}
		catch (Exception seedEx)
		{
			Console.WriteLine($"Seeding failed: {seedEx.Message}");
		}
	}
}
else
{
	Console.WriteLine("InitializeDb - running in-memory validation...");

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

	// CENs
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

	// CPs
	var unirEquipoCP = new UnirEquipoCP(miembroEquipoRepo, usuarioRepo, notRepo, uow);
	var aprobarPropuestaCP = new AprobarPropuestaTorneoCP(propuestaRepo, participacionRepo, uow);

	// Seed users
	var u1 = usuarioCEN.NewUsuario("alice", "alice@example.com", PasswordHasher.Hash("password1"));
	var u2 = usuarioCEN.NewUsuario("bob", "bob@example.com", PasswordHasher.Hash("password2"));

	Console.WriteLine($"Created users: {u1.IdUsuario}={u1.Nick}, {u2.IdUsuario}={u2.Nick}");

	// Login test
	var loginOk = authCEN.Login("alice", "password1") != null;
	Console.WriteLine($"Login alice/password1 -> {(loginOk ? "OK" : "FAIL")}");

	// Seed comunidad and equipo
	var com = comunidadCEN.NewComunidad("Gamers", "Comunidad de prueba");
	var eq = equipoCEN.NewEquipo("TeamA", "Equipo de ejemplo");
	Console.WriteLine($"Created comunidad {com.IdComunidad} and equipo {eq.IdEquipo}");

	// Add member comunidad
	var mc = miembroComunidadCEN.NewMiembroComunidad(u1, com, ApplicationCore.Domain.Enums.RolComunidad.LIDER);
	Console.WriteLine($"MiembroComunidad created: {mc.IdMiembroComunidad} user {mc.Usuario.Nick} in comunidad {mc.Comunidad.Nombre}");

	// Publicacion + comentario + reaccion
	var pub = publicacionCEN.NewPublicacion("Hola mundo!", com, u1);
	var comt = publicacionCEN.AddComentario(pub, u2, "Buen post!");
	var reac = reaccionCEN.NewReaccion(ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA, u2, pub, null);
	Console.WriteLine($"Publicacion {pub.IdPublicacion} with comentario {comt.IdComentario} and reaccion {reac.IdReaccion}");

	// Propuesta torneo + votos + CP approve
	var torneo = new Torneo { Nombre = "Torneo1", FechaInicio = DateTime.UtcNow, Estado = "Planificado" };
	var prop = propuestaCEN.NewPropuestaTorneo(eq, torneo, u1);
	// add two votos si
	prop.Votos.Add(new VotoTorneo { Valor = true, FechaVoto = DateTime.UtcNow, Votante = u1 });
	prop.Votos.Add(new VotoTorneo { Valor = true, FechaVoto = DateTime.UtcNow, Votante = u2 });
	var aprobada = propuestaCEN.AprobarSiVotosUnanimes(prop);
	Console.WriteLine($"Propuesta {prop.IdPropuesta} aprobada por votos unanimes? {aprobada}");

	var aprobadoPorCP = aprobarPropuestaCP.Ejecutar(prop);
	Console.WriteLine($"AprobarPropuestaTorneoCP result: {aprobadoPorCP}");

	// Unir equipo CP
	var miembroEquipo = unirEquipoCP.Ejecutar(u2.IdUsuario, eq, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);
	Console.WriteLine($"MiembroEquipo creado por CP: {miembroEquipo.IdMiembroEquipo}");

	Console.WriteLine("InitializeDb in-memory validation completed.");
}


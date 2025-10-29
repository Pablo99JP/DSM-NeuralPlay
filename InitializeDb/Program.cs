using System;
using System.IO;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Infrastructure.Memory;
using Infrastructure.NHibernate;

// Modes: --mode=inmemory (default) | --mode=schemaexport
// Flags: --force-drop (allow destructive recreate), --confirm (required with --force-drop), --db-name=<name>
string mode = "inmemory";
bool forceDrop = false;
bool confirm = false;
string dbName = "ProjectDatabase";
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
}

if (mode == "schemaexport")
{
	Console.WriteLine("InitializeDb - running NHibernate SchemaExport mode...");
	// Prefer repository-local InitializeDb/Data (relative to the project folder). When running from the build output
	// AppContext.BaseDirectory typically points to bin/.../net8.0, so move up to the project folder and create Data there.
	var repoData = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data"));
	var dataDir = repoData;
	Directory.CreateDirectory(dataDir);

	// Try LocalDB first (recommended by solution.plan.md)
	var mdfPath = Path.Combine(dataDir, dbName + ".mdf");
	var localDbConn = $"Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename={mdfPath};Integrated Security=True;Connect Timeout=30;";
	try
	{
		Console.WriteLine($"Attempting SchemaExport to LocalDB ({Path.GetFileName(mdfPath)})...");

		// Safety: if MDF already exists and user didn't pass --force-drop, skip LocalDB attempt
		if (File.Exists(mdfPath) && !forceDrop)
		{
			Console.WriteLine($"LocalDB MDF already exists at {mdfPath} and --force-drop not provided. Skipping LocalDB attempt and falling back to SQLite.");
			throw new InvalidOperationException("LocalDB MDF exists and force-drop not set.");
		}

		// If forceDrop is requested, require explicit --confirm to perform destructive action
		if (forceDrop && !confirm)
		{
			Console.WriteLine("--force-drop specified but --confirm not provided. Aborting destructive action. Falling back to SQLite.");
			throw new InvalidOperationException("Force drop requested without confirm.");
		}

		// If forceDrop+confirm and MDF exists, attempt to delete existing MDF files before exporting
		if (forceDrop && confirm && File.Exists(mdfPath))
		{
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

		NHibernateHelper.ExportSchema(localDbConn, "NHibernate.Dialect.MsSql2012Dialect");
		Console.WriteLine("SchemaExport to LocalDB completed.");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"LocalDB SchemaExport failed: {ex.Message}");
		Console.WriteLine("Falling back to file-based SQLite SchemaExport...");
		var sqlitePath = Path.Combine(dataDir, "project.db");
		var sqliteConn = $"Data Source={sqlitePath};Version=3;";
		try
		{
			NHibernateHelper.ExportSchema(sqliteConn, "NHibernate.Dialect.SQLiteDialect");
			Console.WriteLine("SchemaExport to SQLite file completed.");
		}
		catch (Exception ex2)
		{
			Console.WriteLine($"SQLite SchemaExport also failed: {ex2.Message}");
			Console.WriteLine("InitializeDb schema export failed. Review NHibernate configuration and environment (LocalDB availability, file permissions).");
			Environment.Exit(1);
		}
	}

	Console.WriteLine("InitializeDb schema export finished.");
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


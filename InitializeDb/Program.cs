using System;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Infrastructure.Memory;

// InitializeDb: use in-memory repositories to seed data and validate CENs/CPs
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


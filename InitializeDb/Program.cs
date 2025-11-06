using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.NHibernate.Repositories;
using Infrastructure.UnitOfWork;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace InitializeDb
{
    /// <summary>
    /// InitializeDb: Aplicación de consola para INICIALIZAR la base de datos.
    /// 
    /// PROPÓSITO:
    /// 1. Crear el esquema de BD desde cero (DROP + CREATE de todas las tablas)
    /// 2. Poblar con datos de prueba (SEED)
    /// 3. Ejecutar PRUEBAS EXHAUSTIVAS de:
    ///    - Custom methods (Login, PromoverAModerador, BanearMiembro, etc.)
    ///    - CPs transaccionales (RegistroUsuarioCP, CrearComunidadCP, etc.)
    ///    - ReadFilters (DamePorEquipo, DamePorComunidad, DamePorTorneo, etc.)
    /// 4. Mostrar resumen final con conteo de entidades
    /// 
    /// CUÁNDO USAR:
    /// - Primer setup del proyecto
    /// - Reiniciar BD desde cero (resetear datos de prueba)
    /// - Verificar que todo funciona correctamente
    /// 
    /// NO USAR EN PRODUCCIÓN: Este programa BORRA TODA LA BD.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Iniciando InitializeDb ===\n");

            try
            {
                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 1: CONFIGURACIÓN DE BASE DE DATOS
                // ═══════════════════════════════════════════════════════════════
                // Intenta conectar a SQL Server Express, si falla usa LocalDB
                Console.WriteLine("1. Configurando base de datos...");
                
                // Connection string para SQL Server Express (instalación típica)
                var connectionString = "Server=localhost\\SQLEXPRESS;Database=ProjectDatabase;Integrated Security=True;";
                
                // Connection string para LocalDB (alternativa si no hay SQL Server Express)
                var localDbConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectDatabase;Integrated Security=True;AttachDBFilename=|DataDirectory|\ProjectDatabase.mdf";

                // Test de conexión: Intenta SQL Server Express primero
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                    }
                    Console.WriteLine("   ✓ Conectado a SQL Server Express.");
                }
                catch (Exception ex)
                {
                    // Si falla SQL Server Express → Fallback a LocalDB
                    Console.WriteLine($"   ✗ No se pudo conectar a SQL Server Express: {ex.Message}");
                    Console.WriteLine("   → Usando LocalDB como fallback...");
                    connectionString = localDbConnectionString;

                    // LocalDB requiere configurar DataDirectory para AttachDBFilename
                    var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
                    if (!Directory.Exists(dataDir))
                    {
                        Directory.CreateDirectory(dataDir);
                    }
                    AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                    Console.WriteLine("   ✓ Configurado LocalDB.");
                }

                // Cargar configuración de NHibernate desde NHibernate.cfg.xml
                var configuration = new Configuration();
                configuration.Configure(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Infrastructure", "NHibernate.cfg.xml"));
                
                // Sobrescribir connection string con la que funcionó (Express o LocalDB)
                configuration.SetProperty("connection.connection_string", connectionString);

                // Cargar TODOS los archivos .hbm.xml (mappings de entidades)
                var mappingsDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Infrastructure", "NHibernate", "Mappings");
                if (Directory.Exists(mappingsDir))
                {
                    foreach (var file in Directory.GetFiles(mappingsDir, "*.hbm.xml"))
                    {
                        configuration.AddFile(file);
                    }
                }

                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 2: CREACIÓN DEL ESQUEMA (DROP + CREATE)
                // ═══════════════════════════════════════════════════════════════
                // SchemaExport: Genera SQL DDL desde los mappings y lo ejecuta
                // CUIDADO: justDrop=false significa DROP + CREATE (borra todo y recrea)
                Console.WriteLine("\n2. Creando esquema de base de datos...");
                var schemaExport = new SchemaExport(configuration);
                
                // Guardar script SQL generado en archivo (útil para debug)
                schemaExport.SetOutputFile(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
                
                // Ejecutar: DROP todas las tablas + CREATE todas las tablas
                schemaExport.Execute(useStdOut: false, execute: true, justDrop: false);
                Console.WriteLine("   ✓ Esquema creado correctamente.\n");

                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 3: INICIALIZAR SESSION FACTORY Y DEPENDENCIAS
                // ═══════════════════════════════════════════════════════════════
                // SessionFactory: Factory para crear sesiones de NHibernate
                // Session: Representa una "unidad de trabajo" con la BD
                Console.WriteLine("3. Inicializando SessionFactory y repositorios...");
                var sessionFactory = configuration.BuildSessionFactory();
                var session = sessionFactory.OpenSession();

                // Crear TODOS los repositorios (comparten la misma sesión)
                // IMPORTANTE: Todos operan sobre la MISMA transacción
                var usuarioRepository = new UsuarioRepository(session);
                var comunidadRepository = new ComunidadRepository(session);
                var equipoRepository = new EquipoRepository(session);
                var miembroComunidadRepository = new MiembroComunidadRepository(session);
                var miembroEquipoRepository = new MiembroEquipoRepository(session);
                var juegoRepository = new JuegoRepository(session);
                var perfilRepository = new PerfilRepository(session);
                var torneoRepository = new TorneoRepository(session);
                var invitacionRepository = new InvitacionRepository(session);
                var solicitudIngresoRepository = new SolicitudIngresoRepository(session);
                var propuestaTorneoRepository = new PropuestaTorneoRepository(session);
                var participacionTorneoRepository = new ParticipacionTorneoRepository(session);
                var publicacionRepository = new PublicacionRepository(session);
                var chatEquipoRepository = new ChatEquipoRepository(session);
                var mensajeChatRepository = new MensajeChatRepository(session);
                var comentarioRepository = new ComentarioRepository(session);
                var reaccionRepository = new ReaccionRepository(session);
                var notificacionRepository = new NotificacionRepository(session);
                var votoTorneoRepository = new VotoTorneoRepository(session);
                var perfilJuegoRepository = new PerfilJuegoRepository(session);
                var sesionRepository = new SesionRepository(session);

                // UnitOfWork: Gestiona las transacciones (SaveChanges = COMMIT)
                var unitOfWork = new NHibernateUnitOfWork(session);

                // Crear TODOS los CENs (componentes de negocio)
                // Cada CEN recibe su repository correspondiente
                var usuarioCEN = new UsuarioCEN(usuarioRepository);
                var comunidadCEN = new ComunidadCEN(comunidadRepository);
                var equipoCEN = new EquipoCEN(equipoRepository);
                var miembroComunidadCEN = new MiembroComunidadCEN(miembroComunidadRepository);
                var miembroEquipoCEN = new MiembroEquipoCEN(miembroEquipoRepository);
                var juegoCEN = new JuegoCEN(juegoRepository);
                var perfilCEN = new PerfilCEN(perfilRepository);
                var torneoCEN = new TorneoCEN(torneoRepository);
                var solicitudIngresoCEN = new SolicitudIngresoCEN(solicitudIngresoRepository, usuarioRepository, comunidadRepository);
                var publicacionCEN = new PublicacionCEN(publicacionRepository);
                var invitacionCEN = new InvitacionCEN(invitacionRepository);
                var chatEquipoCEN = new ChatEquipoCEN(chatEquipoRepository);
                var mensajeChatCEN = new MensajeChatCEN(mensajeChatRepository);
                var comentarioCEN = new ComentarioCEN(comentarioRepository);
                var reaccionCEN = new ReaccionCEN(reaccionRepository);
                var notificacionCEN = new NotificacionCEN(notificacionRepository);
                var propuestaTorneoCEN = new PropuestaTorneoCEN(propuestaTorneoRepository);
                var votoTorneoCEN = new VotoTorneoCEN(votoTorneoRepository);
                var participacionTorneoCEN = new ParticipacionTorneoCEN(participacionTorneoRepository);
                var perfilJuegoCEN = new PerfilJuegoCEN(perfilJuegoRepository);
                var sesionCEN = new SesionCEN(sesionRepository);

                // Crear TODOS los CPs (casos de proceso / use cases transaccionales)
                // Cada CP orquesta múltiples CENs y garantiza transaccionalidad
                var registroUsuarioCP = new RegistroUsuarioCP(usuarioCEN, perfilCEN, unitOfWork);
                var crearComunidadCP = new CrearComunidadCP(comunidadCEN, miembroComunidadCEN, unitOfWork);
                var aceptarInvitacionEquipoCP = new AceptarInvitacionEquipoCP(invitacionRepository, miembroEquipoCEN, unitOfWork);
                var aprobarPropuestaTorneoCP = new AprobarPropuestaTorneoCP(propuestaTorneoRepository, participacionTorneoRepository, unitOfWork);

                Console.WriteLine("   ✓ Componentes inicializados.\n");

                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 4: SEED - CREAR ENTIDADES DE PRUEBA
                // ═══════════════════════════════════════════════════════════════
                // Poblar la BD con datos iniciales para testing
                Console.WriteLine("4. Creando entidades de prueba...");
                Console.WriteLine("\n   --- USUARIOS ---");

                // Usuario 1: Creado con RegistroUsuarioCP (también crea su Perfil)
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CP/RegistroUsuarioCP.cs → RegistrarUsuarioConPerfil()
                var idUsuario1 = registroUsuarioCP.RegistrarUsuarioConPerfil(
                    nick: "player1",
                    correoElectronico: "player1@test.com",
                    contrasenaHash: "hash123",
                    telefono: "123456789"
                );
                Console.WriteLine($"   ✓ Usuario creado: player1 (ID: {idUsuario1})");

                // Usuario 2: Creado con RegistroUsuarioCP (también crea su Perfil)
                var idUsuario2 = registroUsuarioCP.RegistrarUsuarioConPerfil(
                    nick: "player2",
                    correoElectronico: "player2@test.com",
                    contrasenaHash: "hash456"
                );
                Console.WriteLine($"   ✓ Usuario creado: player2 (ID: {idUsuario2})");

                // Usuario 3: Creado SOLO con UsuarioCEN (sin Perfil)
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/UsuarioCEN.cs → Crear()
                // NOTA: Aquí NO usamos CP, por lo que el SaveChanges es manual
                var idUsuario3 = usuarioCEN.Crear(
                    nick: "player3",
                    correoElectronico: "player3@test.com",
                    contrasenaHash: "hash789"
                );
                unitOfWork.SaveChanges(); // Commit manual (porque no usamos CP)
                Console.WriteLine($"   ✓ Usuario creado: player3 (ID: {idUsuario3})");

                Console.WriteLine("\n   --- JUEGOS ---");
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/JuegoCEN.cs → Crear()
                var idJuego1 = juegoCEN.Crear("League of Legends", GeneroJuego.ESTRATEGIA);
                var idJuego2 = juegoCEN.Crear("FIFA 24", GeneroJuego.DEPORTE);
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Juego creado: League of Legends (ID: {idJuego1})");
                Console.WriteLine($"   ✓ Juego creado: FIFA 24 (ID: {idJuego2})");

                Console.WriteLine("\n   --- COMUNIDADES ---");
                var idComunidad1 = crearComunidadCP.CrearComunidadConLider(
                    nombre: "Gamers Pro",
                    descripcion: "Comunidad de jugadores profesionales",
                    idUsuarioLider: idUsuario1
                );
                Console.WriteLine($"   ✓ Comunidad creada: Gamers Pro (ID: {idComunidad1})");

                var idComunidad2 = comunidadCEN.Crear(
                    nombre: "Casual Players",
                    fechaCreacion: DateTime.Now,
                    descripcion: "Para jugar sin presión"
                );
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Comunidad creada: Casual Players (ID: {idComunidad2})");

                Console.WriteLine("\n   --- EQUIPOS ---");
                var idEquipo1 = equipoCEN.Crear("Team Alpha", DateTime.Now, "Equipo competitivo");
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Equipo creado: Team Alpha (ID: {idEquipo1})");

                Console.WriteLine("\n   --- TORNEOS ---");
                var idTorneo1 = torneoCEN.Crear(
                    nombre: "Copa de Verano 2025",
                    fechaInicio: DateTime.Now.AddDays(30),
                    estado: "PENDIENTE",
                    reglas: "Formato eliminación directa"
                );
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Torneo creado: Copa de Verano 2025 (ID: {idTorneo1})");

                Console.WriteLine("\n   --- PUBLICACIONES ---");
                var idPublicacion1 = publicacionCEN.Crear(
                    contenido: "¡Bienvenidos a la comunidad!",
                    fechaCreacion: DateTime.Now
                );
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Publicación creada (ID: {idPublicacion1})");

                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 5: PROBAR MÉTODOS CUSTOM (CEN)
                // ═══════════════════════════════════════════════════════════════
                // Testear que los custom methods funcionan correctamente
                Console.WriteLine("\n5. Probando métodos CUSTOM (CEN)...");

                // TEST 1: Login con autenticación
                Console.WriteLine("\n   --- LOGIN ---");
                try
                {
                    // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/UsuarioCEN.cs (línea ~95) → Login()
                    // Valida: usuario existe + contraseña correcta + cuenta ACTIVA
                    var usuarioLogueado = usuarioCEN.Login("player1@test.com", "hash123");
                    Console.WriteLine($"   ✓ Login exitoso: {usuarioLogueado.Nick}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"   ✗ Login falló: {ex.Message}");
                }

                // TEST 2: Promoción de MIEMBRO → MODERADOR
                Console.WriteLine("\n   --- PROMOCIONAR A MODERADOR ---");
                var idMiembro1 = miembroComunidadCEN.Crear(RolComunidad.MIEMBRO, EstadoMembresia.ACTIVA);
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Miembro creado (ID: {idMiembro1})");
                
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/MiembroComunidadCEN.cs → PromoverAModerador()
                // Cambia Rol de MIEMBRO → MODERADOR
                miembroComunidadCEN.PromoverAModerador(idMiembro1);
                unitOfWork.SaveChanges();
                var miembroPromovido = miembroComunidadCEN.DamePorOID(idMiembro1);
                Console.WriteLine($"   ✓ Miembro promovido a: {miembroPromovido.Rol}");

                // TEST 3: Actualizar fecha de última acción
                Console.WriteLine("\n   --- ACTUALIZAR FECHA ---");
                var nuevaFecha = DateTime.Now.AddDays(-7);
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/MiembroComunidadCEN.cs → ActualizarFechaAccion()
                // Actualiza UltimaAccion para tracking de actividad
                miembroComunidadCEN.ActualizarFechaAccion(idMiembro1, nuevaFecha);
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Fecha actualizada a: {nuevaFecha:yyyy-MM-dd}");

                // TEST 4: Banear miembro de equipo
                Console.WriteLine("\n   --- BANEAR MIEMBRO ---");
                var idMiembroEquipo1 = miembroEquipoCEN.Crear(RolEquipo.MIEMBRO, EstadoMembresia.ACTIVA);
                unitOfWork.SaveChanges();
                Console.WriteLine($"   ✓ Miembro de equipo creado (ID: {idMiembroEquipo1})");
                
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/MiembroEquipoCEN.cs → BanearMiembro()
                // Cambia Estado → EXPULSADA, FechaBaja → DateTime.Now
                miembroEquipoCEN.BanearMiembro(idMiembroEquipo1);
                unitOfWork.SaveChanges();
                var miembroBaneado = miembroEquipoCEN.DamePorOID(idMiembroEquipo1);
                Console.WriteLine($"   ✓ Miembro baneado. Estado: {miembroBaneado.Estado}, Fecha baja: {miembroBaneado.FechaBaja}");

                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 6: PROBAR CUSTOM TRANSACTIONS (CP)
                // ═══════════════════════════════════════════════════════════════
                // Testear que los CPs funcionan correctamente (transaccionalidad)
                Console.WriteLine("\n6. Probando CUSTOM TRANSACTIONS (CP)...");
                
                Console.WriteLine("\n   --- CP: REGISTRO USUARIO ---");
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CP/RegistroUsuarioCP.cs → RegistrarUsuarioConPerfil()
                // Crea Usuario + Perfil en UNA SOLA TRANSACCIÓN
                var idUsuario4 = registroUsuarioCP.RegistrarUsuarioConPerfil(
                    nick: "newplayer",
                    correoElectronico: "newplayer@test.com",
                    contrasenaHash: "newhash"
                );
                Console.WriteLine($"   ✓ CP ejecutado: Usuario + Perfil creados (ID Usuario: {idUsuario4})");

                Console.WriteLine("\n   --- CP: CREAR COMUNIDAD CON LIDER ---");
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CP/CrearComunidadCP.cs → CrearComunidadConLider()
                // Crea Comunidad + MiembroComunidad (líder) en UNA SOLA TRANSACCIÓN
                var idComunidad3 = crearComunidadCP.CrearComunidadConLider(
                    nombre: "Elite Squad",
                    descripcion: "Solo los mejores",
                    idUsuarioLider: idUsuario2
                );
                Console.WriteLine($"   ✓ CP ejecutado: Comunidad + Líder creados (ID Comunidad: {idComunidad3})");

                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 7: PROBAR READFILTERS
                // ═══════════════════════════════════════════════════════════════
                // Testear que los métodos de búsqueda/filtrado funcionan correctamente
                Console.WriteLine("\n7. Probando FILTROS (ReadFilter)...");

                Console.WriteLine("\n   --- FILTRO: Usuarios por nombre ---");
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/UsuarioCEN.cs → DamePorFiltro()
                // Busca usuarios donde Nick o Email contengan "player" (LIKE '%player%')
                var usuariosFiltrados = usuarioCEN.DamePorFiltro("player");
                Console.WriteLine($"   ✓ Usuarios encontrados: {usuariosFiltrados.Count}");
                foreach (var u in usuariosFiltrados.Take(3))
                {
                    Console.WriteLine($"      - {u.Nick} ({u.CorreoElectronico})");
                }

                Console.WriteLine("\n   --- FILTRO: Comunidades por nombre ---");
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/ComunidadCEN.cs → DamePorFiltro()
                // Busca comunidades donde Nombre o Descripción contengan "Gamers"
                var comunidadesFiltradas = comunidadCEN.DamePorFiltro("Gamers");
                Console.WriteLine($"   ✓ Comunidades encontradas: {comunidadesFiltradas.Count}");
                foreach (var c in comunidadesFiltradas)
                {
                    Console.WriteLine($"      - {c.Nombre}");
                }

                Console.WriteLine("\n   --- FILTRO: Equipos por nombre ---");
                // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/EquipoCEN.cs → DamePorFiltro()
                // Busca equipos donde Nombre o Descripción contengan "Team"
                var equiposFiltrados = equipoCEN.DamePorFiltro("Team");
                Console.WriteLine($"   ✓ Equipos encontrados: {equiposFiltrados.Count}");
                foreach (var e in equiposFiltrados)
                {
                    Console.WriteLine($"      - {e.Nombre}");
                }

                Console.WriteLine("\n   --- FILTRO: Torneos por nombre ---");
                // Busca torneos donde Nombre o Estado contengan "Copa"
                var torneosFiltrados = torneoCEN.DamePorFiltro("Copa");
                Console.WriteLine($"   ✓ Torneos encontrados: {torneosFiltrados.Count}");
                foreach (var t in torneosFiltrados)
                {
                    Console.WriteLine($"      - {t.Nombre} ({t.Estado})");
                }

                Console.WriteLine("\n   --- FILTRO: Juegos por nombre ---");
                // Busca juegos donde NombreJuego contenga "FIFA"
                var juegosFiltrados = juegoCEN.DamePorFiltro("FIFA");
                Console.WriteLine($"   ✓ Juegos encontrados: {juegosFiltrados.Count}");
                foreach (var j in juegosFiltrados)
                {
                    Console.WriteLine($"      - {j.NombreJuego} ({j.Genero})");
                }

                // ═══════════════════════════════════════════════════════════════
                // SECCIÓN 8: RESUMEN FINAL
                // ═══════════════════════════════════════════════════════════════
                // Mostrar conteo total de entidades creadas
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("=== RESUMEN DE INICIALIZACIÓN ===");
                Console.WriteLine(new string('=', 60));
                
                // Contar entidades usando DameTodos() (SELECT * FROM Tabla)
                Console.WriteLine($"✓ Usuarios creados: {usuarioCEN.DameTodos().Count}");
                Console.WriteLine($"✓ Comunidades creadas: {comunidadCEN.DameTodos().Count}");
                Console.WriteLine($"✓ Equipos creados: {equipoCEN.DameTodos().Count}");
                Console.WriteLine($"✓ Juegos creados: {juegoCEN.DameTodos().Count}");
                Console.WriteLine($"✓ Torneos creados: {torneoCEN.DameTodos().Count}");
                Console.WriteLine($"✓ Miembros comunidad: {miembroComunidadCEN.DameTodos().Count}");
                Console.WriteLine($"✓ Miembros equipo: {miembroEquipoCEN.DameTodos().Count}");
                Console.WriteLine($"✓ Publicaciones: {publicacionCEN.DameTodos().Count}");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("\n✓✓✓ InitializeDb COMPLETADO EXITOSAMENTE ✓✓✓\n");

                // Liberar recursos de NHibernate
                session.Dispose();
                sessionFactory.Dispose();
            }
            catch (Exception ex)
            {
                // Capturar y mostrar CUALQUIER error durante la inicialización
                Console.WriteLine($"\n✗✗✗ ERROR durante la inicialización ✗✗✗");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}

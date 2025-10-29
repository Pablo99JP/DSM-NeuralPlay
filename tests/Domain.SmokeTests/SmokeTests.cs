using System;
using Xunit;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class SmokeTests
    {
        [Fact]
        public void CENs_And_CPs_Work_Together()
        {
            var usuarioRepo = new InMemoryUsuarioRepository();
            var comunidadRepo = new InMemoryRepository<Comunidad>();
            var equipoRepo = new InMemoryRepository<Equipo>();
            var publicacionRepo = new InMemoryRepository<Publicacion>();
            var comentarioRepo = new InMemoryRepository<Comentario>();
            var reaccionRepo = new InMemoryRepository<Reaccion>();
            var propuestaRepo = new InMemoryRepository<PropuestaTorneo>();
            var miembroComunidadRepo = new InMemoryRepository<MiembroComunidad>();
            var miembroEquipoRepo = new InMemoryRepository<MiembroEquipo>();
            var notRepo = new InMemoryRepository<Notificacion>();
            var participacionRepo = new InMemoryRepository<ParticipacionTorneo>();
            var uow = new InMemoryUnitOfWork();

            var usuarioCEN = new UsuarioCEN(usuarioRepo);
            var authCEN = new AuthenticationCEN(usuarioRepo);
            var comunidadCEN = new ComunidadCEN(comunidadRepo);
            var publicacionCEN = new PublicacionCEN(publicacionRepo);
            var comentarioCEN = new ComentarioCEN(comentarioRepo);
            var reaccionCEN = new ReaccionCEN(reaccionRepo);
            var propuestaCEN = new PropuestaTorneoCEN(propuestaRepo);
            var miembroComunidadCEN = new MiembroComunidadCEN(miembroComunidadRepo);
            var unirEquipoCP = new ApplicationCore.Domain.CP.UnirEquipoCP(miembroEquipoRepo, usuarioRepo, notRepo, uow);
            var aprobarCP = new ApplicationCore.Domain.CP.AprobarPropuestaTorneoCP(propuestaRepo, participacionRepo, uow);

            // create users
            var u1 = usuarioCEN.NewUsuario("test1", "t1@example.com", ApplicationCore.Domain.CEN.PasswordHasher.Hash("p1"));
            var u2 = usuarioCEN.NewUsuario("test2", "t2@example.com", ApplicationCore.Domain.CEN.PasswordHasher.Hash("p2"));

            Assert.NotNull(u1);
            Assert.NotNull(u2);

            // login
            var ok = authCEN.Login("test1", "p1");
            Assert.NotNull(ok);

            // comunidad
            var com = comunidadCEN.NewComunidad("TestCom");
            Assert.NotNull(com);

            // miembro comunidad
            var mc = miembroComunidadCEN.NewMiembroComunidad(u1, com, ApplicationCore.Domain.Enums.RolComunidad.LIDER);
            Assert.Equal(ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, mc.Estado);

            // publicacion + comentario + reaccion
            var pub = publicacionCEN.NewPublicacion("Hello", com, u1);
            var comt = publicacionCEN.AddComentario(pub, u2, "Nice");
            var r = reaccionCEN.NewReaccion(ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA, u2, pub, null);
            Assert.NotNull(pub);
            Assert.NotNull(comt);
            Assert.NotNull(r);

            // propuesta torneo and approve via CEN + CP
            var eq = new Equipo { Nombre = "Eq1", FechaCreacion = DateTime.UtcNow };
            equipoRepo.New(eq);
            var torneo = new Torneo { Nombre = "T1", FechaInicio = DateTime.UtcNow, Estado = "Open" };
            var prop = propuestaCEN.NewPropuestaTorneo(eq, torneo, u1);
            prop.Votos.Add(new VotoTorneo { Valor = true, FechaVoto = DateTime.UtcNow, Votante = u1 });
            prop.Votos.Add(new VotoTorneo { Valor = true, FechaVoto = DateTime.UtcNow, Votante = u2 });
            var apro = propuestaCEN.AprobarSiVotosUnanimes(prop);
            Assert.True(apro);

            var aprobCp = aprobarCP.Ejecutar(prop);
            Assert.True(aprobCp);

            // unir equipo via CP
            var miembroEq = unirEquipoCP.Ejecutar(u2.IdUsuario, eq, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);
            Assert.NotNull(miembroEq);
        }
    }
}

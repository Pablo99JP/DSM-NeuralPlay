using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateSesionInvitacionTests
    {
        [Fact]
        public void Can_Save_And_Read_Sesion_And_Invitacion_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var usuarioRepo = new NHibernateUsuarioRepository(session);
            var sesionRepo = new NHibernateSesionRepository(session);
            var invitacionRepo = new NHibernateInvitacionRepository(session);
            var comunidadRepo = new NHibernateComunidadRepository(session);
            var equipoRepo = new NHibernateEquipoRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var usuario = new Usuario { Nick = "SessUser", CorreoElectronico = "sess@example.com" };
            usuarioRepo.New(usuario);

            var sesion = new Sesion { Usuario = usuario, FechaInicio = DateTime.UtcNow, Token = Guid.NewGuid().ToString() };
            sesionRepo.New(sesion);

            var comunidad = new Comunidad { Nombre = "ComTest" };
            comunidadRepo.New(comunidad);

            var equipo = new Equipo { Nombre = "EqTest" };
            equipoRepo.New(equipo);

            var invitacion = new Invitacion { Emisor = usuario, Destinatario = usuario, Tipo = ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE, FechaEnvio = DateTime.UtcNow, Comunidad = comunidad };
            invitacionRepo.New(invitacion);

            uow.SaveChanges();

            var loadedS = sesionRepo.ReadById(sesion.IdSesion);
            Assert.NotNull(loadedS);

            var loadedI = invitacionRepo.ReadById(invitacion.IdInvitacion);
            Assert.NotNull(loadedI);
            Assert.Equal(ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD, loadedI!.Tipo);
        }
    }
}

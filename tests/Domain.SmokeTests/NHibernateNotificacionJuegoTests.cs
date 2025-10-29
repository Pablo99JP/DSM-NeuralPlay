using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateNotificacionJuegoTests
    {
        [Fact]
        public void Can_Save_And_Read_Notificacion_And_Juego_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var usuarioRepo = new NHibernateUsuarioRepository(session);
            var notificacionRepo = new NHibernateNotificacionRepository(session);
            var juegoRepo = new NHibernateJuegoRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var usuario = new Usuario { Nick = "NotUser", CorreoElectronico = "not@example.com" };
            usuarioRepo.New(usuario);

            var juego = new Juego { NombreJuego = "JuegoNH", Genero = ApplicationCore.Domain.Enums.GeneroJuego.ACCION };
            juegoRepo.New(juego);

            var notificacion = new Notificacion { Destinatario = usuario, Tipo = ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, Mensaje = "Prueba notif", Leida = false, FechaCreacion = DateTime.UtcNow };
            notificacionRepo.New(notificacion);

            uow.SaveChanges();

            var loadedJ = juegoRepo.ReadById(juego.IdJuego);
            Assert.NotNull(loadedJ);
            Assert.Equal("JuegoNH", loadedJ!.NombreJuego);

            var loadedN = notificacionRepo.ReadById(notificacion.IdNotificacion);
            Assert.NotNull(loadedN);
            Assert.Equal("Prueba notif", loadedN!.Mensaje);
        }
    }
}

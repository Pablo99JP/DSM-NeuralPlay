using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateSolicitudChatTests
    {
        [Fact]
        public void Can_Save_And_Read_SolicitudIngreso_ChatEquipo_MensajeChat_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var usuarioRepo = new NHibernateUsuarioRepository(session);
            var solicitudRepo = new NHibernateSolicitudIngresoRepository(session);
            var chatRepo = new NHibernateChatEquipoRepository(session);
            var mensajeRepo = new NHibernateMensajeChatRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var usuario = new Usuario { Nick = "ReqUser", CorreoElectronico = "req@example.com" };
            usuarioRepo.New(usuario);

            var solicitud = new SolicitudIngreso { Solicitante = usuario, Tipo = ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE, FechaSolicitud = DateTime.UtcNow };
            solicitudRepo.New(solicitud);

            var chat = new ChatEquipo();
            chatRepo.New(chat);

            var mensaje = new MensajeChat { Chat = chat, Autor = usuario, Contenido = "Hola chat", FechaEnvio = DateTime.UtcNow };
            mensajeRepo.New(mensaje);

            uow.SaveChanges();

            var loadedS = solicitudRepo.ReadById(solicitud.IdSolicitud);
            Assert.NotNull(loadedS);

            var loadedM = mensajeRepo.ReadById(mensaje.IdMensajeChat);
            Assert.NotNull(loadedM);
            Assert.Equal("Hola chat", loadedM!.Contenido);
        }
    }
}

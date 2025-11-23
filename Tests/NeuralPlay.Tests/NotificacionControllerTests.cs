using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.CEN;
using NeuralPlay.Controllers;
using ApplicationCore.Domain.EN;
using Moq;

namespace NeuralPlay.Tests
{
    public class NotificacionControllerTests
    {
        [Fact]
        public void Index_RedirectsToLogin_WhenNotAuthenticated()
        {
            var repo = new InMemoryRepository<Notificacion>();
            var notCen = new NotificacionCEN(repo);
            var userRepo = new InMemoryUsuarioRepository();
            var userCen = new UsuarioCEN(userRepo);

            var controller = new NotificacionController(userCen, userRepo, notCen);

            var result = controller.Index() as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("Login", result!.ActionName);
            Assert.Equal("Usuario", result.ControllerName);
        }

        [Fact]
        public void Index_ReturnsOnlyUserNotifications()
        {
            var repo = new InMemoryRepository<Notificacion>();
            var userRepo = new InMemoryUsuarioRepository();
            var userCen = new UsuarioCEN(userRepo);
            var notCen = new NotificacionCEN(repo);

            // Simular usuario autenticado
            var usuario = userCen.NewUsuario("TestUser", "test@user.com", "pwd");
            var controller = new NotificacionController(userCen, userRepo, notCen);

            var context = new DefaultHttpContext();
            var sessionMock = new Moq.Mock<ISession>();
#pragma warning disable CS8601
            sessionMock.Setup(s => s.TryGetValue("UsuarioId", out It.Ref<byte[]>.IsAny))
                .Callback((string key, out byte[] value) => {
                    value = BitConverter.GetBytes((int)usuario.IdUsuario);
                })
                .Returns(true);
#pragma warning restore CS8601
            context.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Crear notificaciones usando el CEN para asegurar persistencia y lógica real
            notCen.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, "Mensaje 1", usuario);
            notCen.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, "Mensaje 2", usuario);
            var otroUsuario = userCen.NewUsuario("Otro", "otro@user.com", "pwd");
            notCen.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, "Mensaje 3", otroUsuario);

            var result = controller.Index() as ViewResult;
            Assert.NotNull(result);
            var model = result!.Model as IList<NeuralPlay.Models.NotificacionViewModel>;
            Assert.NotNull(model);
            Assert.Equal(2, model!.Count);
            Assert.All(model, n => Assert.Equal((int)usuario.IdUsuario, n.UsuarioId));
        }
    }
}

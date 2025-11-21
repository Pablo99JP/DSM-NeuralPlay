using Microsoft.AspNetCore.Mvc;
using Xunit;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.CEN;
using NeuralPlay.Controllers;
using ApplicationCore.Domain.EN;

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
    }
}

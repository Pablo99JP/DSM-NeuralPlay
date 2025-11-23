using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.CEN;
using NeuralPlay.Controllers;
using NeuralPlay.Models;
using Moq;

namespace NeuralPlay.Tests
{
    public class UsuarioControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithEmptyListInitially()
        {
            var repo = new InMemoryUsuarioRepository();
            var cen = new UsuarioCEN(repo);
            var unitOfWorkMock = new Mock<ApplicationCore.Domain.Repositories.IUnitOfWork>();
            var controller = new UsuarioController(cen, repo, unitOfWorkMock.Object);

            var result = controller.Index() as ViewResult;
            Assert.NotNull(result);

            var model = result!.Model as System.Collections.Generic.IList<UsuarioViewModel>;
            Assert.NotNull(model);
            Assert.Empty(model!);
        }

        [Fact]
        public void Create_Post_AddsUserAndRedirects()
        {
            var repo = new InMemoryUsuarioRepository();
            var cen = new UsuarioCEN(repo);
            var unitOfWorkMock = new Mock<ApplicationCore.Domain.Repositories.IUnitOfWork>();
            var controller = new UsuarioController(cen, repo, unitOfWorkMock.Object);

            var vm = new UsuarioViewModel { Nombre = "Test", Email = "t@t.com", Password = "pwd" };

            var result = controller.Create(vm) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);

            var all = repo.ReadAll().ToList();
            Assert.Single(all);
            Assert.Equal("Test", all[0].Nick);
            Assert.Equal("t@t.com", all[0].CorreoElectronico);
        }

        [Fact]
        public void Edit_Post_UpdatesUser()
        {
            var repo = new InMemoryUsuarioRepository();
            var cen = new UsuarioCEN(repo);
            var unitOfWorkMock = new Mock<ApplicationCore.Domain.Repositories.IUnitOfWork>();
            var controller = new UsuarioController(cen, repo, unitOfWorkMock.Object);

            // create initial
            var created = cen.NewUsuario("OldName", "old@e.com", "pwd");
            var id = created.IdUsuario;

            var vm = new UsuarioViewModel { Id = (int)id, Nombre = "NewName", Email = "new@e.com", Password = "newpwd" };

            var result = controller.Edit(vm) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);

            var en = repo.ReadById(id);
            Assert.NotNull(en);
            Assert.Equal("NewName", en!.Nick);
            Assert.Equal("new@e.com", en.CorreoElectronico);
        }

        [Fact]
        public void Delete_Post_RemovesUser()
        {
            var repo = new InMemoryUsuarioRepository();
            var cen = new UsuarioCEN(repo);
            var unitOfWorkMock = new Mock<ApplicationCore.Domain.Repositories.IUnitOfWork>();
            var controller = new UsuarioController(cen, repo, unitOfWorkMock.Object);

            var created = cen.NewUsuario("ToDelete", "d@d.com", "pwd");
            var id = created.IdUsuario;

            var result = controller.Delete(id) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);

            var en = repo.ReadById(id);
            Assert.Null(en);
        }

        [Fact]
        public void Details_ReturnsViewWithModel()
        {
            var repo = new InMemoryUsuarioRepository();
            var cen = new UsuarioCEN(repo);
            var unitOfWorkMock = new Mock<ApplicationCore.Domain.Repositories.IUnitOfWork>();
            var controller = new UsuarioController(cen, repo, unitOfWorkMock.Object);

            var created = cen.NewUsuario("DetailName", "z@z.com", "pwd");
            var id = created.IdUsuario;

            var result = controller.Details(id) as ViewResult;
            Assert.NotNull(result);

            var model = result!.Model as UsuarioViewModel;
            Assert.NotNull(model);
            Assert.Equal((int)id, model!.Id);
        }

        [Fact]
        public void Register_ShouldStoreHashedPassword()
        {
            var repo = new InMemoryUsuarioRepository();
            var cen = new UsuarioCEN(repo);
            var unitOfWorkMock = new Mock<ApplicationCore.Domain.Repositories.IUnitOfWork>();
            var controller = new UsuarioController(cen, repo, unitOfWorkMock.Object);

            var vm = new UsuarioViewModel { Nombre = "HashTest", Email = "hash@t.com", Password = "secretpwd" };

            var result = controller.Create(vm) as RedirectToActionResult;
            Assert.NotNull(result);

            var all = repo.ReadAll().ToList();
            Assert.Single(all);
            var stored = all[0];

            // Verificar que la contraseña no se almacena en claro y que el PBKDF2 la verifica correctamente
            Assert.NotEqual(vm.Password, stored.ContrasenaHash);
            Assert.True(ApplicationCore.Domain.CEN.PasswordHasher.Verify(vm.Password, stored.ContrasenaHash));
        }
    }
}

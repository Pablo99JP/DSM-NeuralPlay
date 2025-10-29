using System;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Infrastructure.Memory;
using Xunit;

namespace Domain.SmokeTests
{
    public class UnirEquipoCPTests
    {
        [Fact]
        public void Ejecutar_AddsMemberAndNotification_WhenUserExists()
        {
            var usuarioRepo = new InMemoryUsuarioRepository();
            var miembroRepo = new InMemoryRepository<MiembroEquipo>();
            var notRepo = new InMemoryRepository<Notificacion>();
            var uow = new InMemoryUnitOfWork();

            var user = new Usuario { Nick = "player1", CorreoElectronico = "p@example.com" };
            usuarioRepo.New(user);

            var equipo = new Equipo { Nombre = "TeamA" };

            var cp = new UnirEquipoCP(miembroRepo, usuarioRepo, notRepo, uow);
            var miembro = cp.Ejecutar(user.IdUsuario, equipo, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);

            Assert.NotNull(miembro);
            Assert.Equal(user.IdUsuario, miembro.Usuario?.IdUsuario);
            // Notification created
            var nots = notRepo.ReadAll();
            Assert.Contains(nots, n => n.Destinatario?.IdUsuario == user.IdUsuario);
        }

        [Fact]
        public void Ejecutar_Throws_WhenUserMissing()
        {
            var usuarioRepo = new InMemoryUsuarioRepository();
            var miembroRepo = new InMemoryRepository<MiembroEquipo>();
            var notRepo = new InMemoryRepository<Notificacion>();
            var uow = new InMemoryUnitOfWork();

            var equipo = new Equipo { Nombre = "TeamA" };
            var cp = new UnirEquipoCP(miembroRepo, usuarioRepo, notRepo, uow);

            Assert.Throws<System.Exception>(() => cp.Ejecutar(9999, equipo, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO));
        }
    }
}

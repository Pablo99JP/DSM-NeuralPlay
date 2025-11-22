using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace NeuralPlay.Tests
{
    public class NHibernateCrudTests
    {
        [Fact]
        public void Usuario_CRUD_Works_With_NHibernate()
        {
            // Arrange
            var session = NHibernateHelper.OpenSession();
            var repo = new Infrastructure.NHibernate.NHibernateUsuarioRepository(session);

            var email = $"testuser_{Guid.NewGuid():N}@example.com";
            var usuario = new Usuario
            {
                Nick = "TestUser",
                CorreoElectronico = email,
                ContrasenaHash = "initialhash",
                FechaRegistro = DateTime.UtcNow,
                EstadoCuenta = ApplicationCore.Domain.Enums.EstadoCuenta.ACTIVO
            };

            // Act & Assert
            using (var tx = session.BeginTransaction())
            {
                repo.New(usuario);
                tx.Commit();
            }

            // Read
            var read = repo.ReadByEmail(email);
            Assert.NotNull(read);
            Assert.Equal("TestUser", read!.Nick);

            // Update
            read.Nick = "UpdatedUser";
            using (var tx = session.BeginTransaction())
            {
                repo.Modify(read);
                tx.Commit();
            }

            var updated = repo.ReadById(read.IdUsuario);
            Assert.NotNull(updated);
            Assert.Equal("UpdatedUser", updated!.Nick);

            // Delete
            using (var tx = session.BeginTransaction())
            {
                repo.Destroy(updated.IdUsuario);
                tx.Commit();
            }

            var afterDelete = repo.ReadById(updated.IdUsuario);
            Assert.Null(afterDelete);

            session.Close();
        }
    }
}

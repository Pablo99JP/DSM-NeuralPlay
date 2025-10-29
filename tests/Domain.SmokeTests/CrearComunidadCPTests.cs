using System;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Infrastructure.Memory;
using Xunit;

namespace Domain.SmokeTests
{
    public class CrearComunidadCPTests
    {
        [Fact]
        public void Ejecutar_CreatesComunidad_WithGivenName()
        {
            var comunidadRepo = new InMemoryRepository<Comunidad>();
            var uow = new InMemoryUnitOfWork();

            var cp = new CrearComunidadCP(comunidadRepo, uow);
            var c = cp.Ejecutar("Gamers", "Comunidad de prueba");

            Assert.NotNull(c);
            Assert.Equal("Gamers", c.Nombre);
            Assert.False(c.FechaCreacion == default);
            // Verify repository stored it
            var read = comunidadRepo.ReadById(c.IdComunidad);
            Assert.NotNull(read);
            Assert.Equal("Gamers", read.Nombre);
        }

        [Fact]
        public void Ejecutar_AllowsNullDescription()
        {
            var comunidadRepo = new InMemoryRepository<Comunidad>();
            var uow = new InMemoryUnitOfWork();

            var cp = new CrearComunidadCP(comunidadRepo, uow);
            var c = cp.Ejecutar("SinDesc");

            Assert.NotNull(c);
            Assert.Equal("SinDesc", c.Nombre);
            Assert.Null(c.Descripcion);
        }
    }
}

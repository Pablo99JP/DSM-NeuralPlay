using System;
using Moq;
using Xunit;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

#nullable enable

namespace Domain.UnitTests
{
    public class CrearComunidadCPTests
    {
        [Fact]
        public void Ejecutar_ShouldCreateComunidad_AndCallRepoAndSave()
        {
            // Arrange
            var repoMock = new Mock<IRepository<Comunidad>>();
            var uowMock = new Mock<IUnitOfWork>();

            Comunidad? captured = null;
        repoMock.Setup(r => r.New(It.IsAny<Comunidad>()))
            .Callback<Comunidad>(c => captured = c);

        var sut = new CrearComunidadCP(repoMock.Object, uowMock.Object);
            var nombre = "ComunidadPrueba";
            var descripcion = "Descripcion de prueba";

            // Act
            var result = sut.Ejecutar(nombre, descripcion);

            // Assert
            // Repo.New should be called once with a Comunidad whose Nombre matches
            repoMock.Verify(r => r.New(It.IsAny<Comunidad>()), Times.Once);
            uowMock.Verify(u => u.SaveChanges(), Times.Once);

            Assert.NotNull(captured);
            Assert.Equal(nombre, captured!.Nombre);
            Assert.Equal(descripcion, captured.Descripcion);

            // The returned object should also have the same values
            Assert.Equal(nombre, result.Nombre);
            Assert.Equal(descripcion, result.Descripcion);
            Assert.True((DateTime.UtcNow - result.FechaCreacion).TotalSeconds < 30, "FechaCreacion should be recent");
        }
    }
}

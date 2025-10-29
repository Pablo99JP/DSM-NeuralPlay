using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateIntegrationTests
    {
        [Fact]
        public void Can_Save_And_Read_Comunidad_And_Equipo_With_NHibernate()
        {
            // Open session from helper
            using var session = NHibernateHelper.OpenSession();

            // Create repositories and unit of work
            var comunidadRepo = new NHibernateComunidadRepository(session);
            var equipoRepo = new NHibernateEquipoRepository(session);
            using var uow = new NHibernateUnitOfWork(session);

            var comunidad = new Comunidad { Nombre = "NHTest", FechaCreacion = System.DateTime.UtcNow, Descripcion = "desc" };
            comunidadRepo.New(comunidad);

            var equipo = new Equipo { Nombre = "EqNH", FechaCreacion = System.DateTime.UtcNow, Comunidad = comunidad };
            equipoRepo.New(equipo);

            uow.SaveChanges();

            // Validate
            var loaded = comunidadRepo.ReadById(comunidad.IdComunidad);
            Assert.NotNull(loaded);
            Assert.Equal("NHTest", loaded!.Nombre);

            var loadedEq = equipoRepo.ReadById(equipo.IdEquipo);
            Assert.NotNull(loadedEq);
            Assert.Equal("EqNH", loadedEq!.Nombre);
        }
    }
}

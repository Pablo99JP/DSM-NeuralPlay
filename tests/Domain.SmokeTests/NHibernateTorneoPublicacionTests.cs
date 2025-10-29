using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateTorneoPublicacionTests
    {
        [Fact]
        public void Can_Save_And_Read_Torneo_And_Publicacion_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var torneoRepo = new NHibernateTorneoRepository(session);
            var publicacionRepo = new NHibernatePublicacionRepository(session);
            using var uow = new NHibernateUnitOfWork(session);

            var torneo = new Torneo { Nombre = "TorneoNH", FechaInicio = DateTime.UtcNow, Estado = "Scheduled" };
            torneoRepo.New(torneo);

            var publicacion = new Publicacion { Contenido = "Hola NH", FechaCreacion = DateTime.UtcNow };
            publicacionRepo.New(publicacion);

            uow.SaveChanges();

            var loadedT = torneoRepo.ReadById(torneo.IdTorneo);
            Assert.NotNull(loadedT);
            Assert.Equal("TorneoNH", loadedT!.Nombre);

            var loadedP = publicacionRepo.ReadById(publicacion.IdPublicacion);
            Assert.NotNull(loadedP);
            Assert.Equal("Hola NH", loadedP!.Contenido);
        }
    }
}

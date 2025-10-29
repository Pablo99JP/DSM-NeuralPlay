using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernatePropuestaParticipacionTests
    {
        [Fact]
        public void Can_Save_And_Read_Propuesta_And_Participacion_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var torneoRepo = new NHibernateTorneoRepository(session);
            var equipoRepo = new NHibernateEquipoRepository(session);
            var propuestaRepo = new NHibernatePropuestaTorneoRepository(session);
            var participacionRepo = new NHibernateParticipacionTorneoRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var torneo = new Torneo { Nombre = "TorneoPropPart", FechaInicio = DateTime.UtcNow, Estado = "Open" };
            torneoRepo.New(torneo);

            var equipo = new Equipo { Nombre = "EquipoNH" };
            equipoRepo.New(equipo);

            var participacion = new ParticipacionTorneo { Torneo = torneo, Equipo = equipo, Estado = "Inscrito", FechaAlta = DateTime.UtcNow };
            participacionRepo.New(participacion);

            var propuesta = new PropuestaTorneo { Torneo = torneo, EquipoProponente = equipo, FechaPropuesta = DateTime.UtcNow, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE };
            propuestaRepo.New(propuesta);

            uow.SaveChanges();

            var loadedPart = participacionRepo.ReadById(participacion.IdParticipacion);
            Assert.NotNull(loadedPart);
            Assert.Equal("Inscrito", loadedPart!.Estado);

            var loadedProp = propuestaRepo.ReadById(propuesta.IdPropuesta);
            Assert.NotNull(loadedProp);
            Assert.Equal(propuesta.EquipoProponente!.Nombre, loadedProp!.EquipoProponente!.Nombre);
        }
    }
}

using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateVotoComentarioTests
    {
        [Fact]
        public void Can_Save_And_Read_Voto_And_Comentario_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var torneoRepo = new NHibernateTorneoRepository(session);
            var propuestaRepo = new NHibernatePropuestaTorneoRepository(session);
            var votoRepo = new NHibernateVotoTorneoRepository(session);
            var publicacionRepo = new NHibernatePublicacionRepository(session);
            var comentarioRepo = new NHibernateComentarioRepository(session);
            var equipoRepo = new NHibernateEquipoRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var torneo = new Torneo { Nombre = "TorneoVotoCom", FechaInicio = DateTime.UtcNow, Estado = "Open" };
            torneoRepo.New(torneo);

            var equipo = new Equipo { Nombre = "EquipoVoto" };
            equipoRepo.New(equipo);

            var propuesta = new PropuestaTorneo { Torneo = torneo, EquipoProponente = equipo, FechaPropuesta = DateTime.UtcNow, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE };
            propuestaRepo.New(propuesta);

            var voto = new VotoTorneo { Propuesta = propuesta, Votante = null, Valor = true, FechaVoto = DateTime.UtcNow };
            votoRepo.New(voto);

            var publicacion = new Publicacion { Contenido = "Post NH", FechaCreacion = DateTime.UtcNow };
            publicacionRepo.New(publicacion);

            var comentario = new Comentario { Contenido = "Buen post", FechaCreacion = DateTime.UtcNow, Autor = null, Publicacion = publicacion };
            comentarioRepo.New(comentario);

            uow.SaveChanges();

            var loadedV = votoRepo.ReadById(voto.IdVoto);
            Assert.NotNull(loadedV);

            var loadedC = comentarioRepo.ReadById(comentario.IdComentario);
            Assert.NotNull(loadedC);
            Assert.Equal("Buen post", loadedC!.Contenido);
        }
    }
}

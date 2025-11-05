using System;
using System.Linq;
using Xunit;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;

namespace Domain.UnitTests
{
    public class CPTests
    {
        [Fact]
        public void AceptarInvitacion_CreatesMiembroAndUpdatesInvitacion()
        {
            var invRepo = new InMemoryInvitacionRepository();
            var miembroRepo = new InMemoryMiembroEquipoRepository();
            var uow = new InMemoryUnitOfWork();

            var user = new Usuario { IdUsuario = 10, Nick = "dest" };
            var equipo = new Equipo { IdEquipo = 20, Nombre = "EquipoX" };

            var invitacion = new Invitacion { Tipo = ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE, FechaEnvio = DateTime.UtcNow, Destinatario = user, Equipo = equipo };
            invRepo.New(invitacion);

            var cp = new AceptarInvitacionCP(invRepo, miembroRepo, uow);
            var miembro = cp.Ejecutar(invitacion);

            var miembros = miembroRepo.ReadAll().ToList();
            Assert.Single(miembros);
            Assert.Equal(user.IdUsuario, miembros[0].Usuario.IdUsuario);
            Assert.Equal(equipo.IdEquipo, miembros[0].Equipo.IdEquipo);

            var storedInv = invRepo.ReadById(invitacion.IdInvitacion);
            Assert.Equal(ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA, storedInv!.Estado);
            Assert.NotNull(storedInv.FechaRespuesta);
        }

        [Fact]
        public void AprobarPropuestaTorneo_CreatesParticipacionAndAcceptsProposal()
        {
            var propRepo = new InMemoryPropuestaTorneoRepository();
            var partRepo = new InMemoryParticipacionTorneoRepository();
            var uow = new InMemoryUnitOfWork();

            var equipo = new Equipo { IdEquipo = 30, Nombre = "E30" };
            var torneo = new Torneo { IdTorneo = 300, Nombre = "T300" };

            var propuesta = new PropuestaTorneo { EquipoProponente = equipo, Torneo = torneo, FechaPropuesta = DateTime.UtcNow, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE };
            var voto = new VotoTorneo { Valor = true, FechaVoto = DateTime.UtcNow, Votante = new Usuario { IdUsuario = 99, Nick = "votante" }, Propuesta = propuesta };
            propuesta.Votos.Add(voto);

            propRepo.New(propuesta);

            var cp = new ApplicationCore.Domain.CP.AprobarPropuestaTorneoCP(propRepo, partRepo, uow);
            var ok = cp.Ejecutar(propuesta);

            Assert.True(ok);
            var parts = partRepo.ReadAll().ToList();
            Assert.Single(parts);
            Assert.Equal(equipo.IdEquipo, parts[0].Equipo.IdEquipo);
            Assert.Equal(torneo.IdTorneo, parts[0].Torneo.IdTorneo);

            var stored = propRepo.ReadById(propuesta.IdPropuesta);
            Assert.Equal(ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA, stored!.Estado);
        }
    }
}

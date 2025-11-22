using System.Linq;
using Moq;
using Xunit;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Repositories;
using EN = ApplicationCore.Domain.EN;

namespace PropuestaTorneo.UnitTests
{
    public class PropuestaTorneoTests
    {
        [Fact]
        public void SiTodosVotanSi_SeCreaParticipacion_Y_SeNotifica()
        {
            // Arrange
            var propuesta = new EN.PropuestaTorneo { IdPropuesta = 1, Votos = new System.Collections.Generic.List<EN.VotoTorneo>() };
            var torneo = new EN.Torneo { IdTorneo = 10, Nombre = "Copa Invierno LoL" };
            propuesta.Torneo = torneo;
            var equipo = new EN.Equipo { IdEquipo = 100, Nombre = "Tigres Arkham" };
            propuesta.EquipoProponente = equipo;

            var u1 = new EN.Usuario { IdUsuario = 201, Nick = "user1" };
            var u2 = new EN.Usuario { IdUsuario = 202, Nick = "user2" };
            var u3 = new EN.Usuario { IdUsuario = 203, Nick = "user3" };

            var miembroRepoMock = new Mock<IMiembroEquipoRepository>();
            miembroRepoMock.Setup(m => m.GetUsuariosByEquipo(equipo.IdEquipo)).Returns(new[] { u1, u2, u3 });

            var propuestaRepoMock = new Mock<IRepository<EN.PropuestaTorneo>>();
            propuestaRepoMock.Setup(p => p.ReadById(propuesta.IdPropuesta)).Returns(propuesta);

            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            usuarioRepoMock.Setup(u => u.ReadById(u1.IdUsuario)).Returns(u1);
            usuarioRepoMock.Setup(u => u.ReadById(u2.IdUsuario)).Returns(u2);
            usuarioRepoMock.Setup(u => u.ReadById(u3.IdUsuario)).Returns(u3);

            var votoRepoMock = new Mock<IRepository<EN.VotoTorneo>>();
            // When New is called, add the vote to propuesta.Votos to simulate persistence
            votoRepoMock.Setup(v => v.New(It.IsAny<EN.VotoTorneo>())).Callback<EN.VotoTorneo>(v => propuesta.Votos.Add(v));

            var participacionRepoMock = new Mock<ApplicationCore.Domain.Repositories.IParticipacionTorneoRepository>();
            var participacionCEN = new ParticipacionTorneoCEN(participacionRepoMock.Object);

            var notRepoMock = new Mock<IRepository<EN.Notificacion>>();
            var notificacionCEN = new NotificacionCEN(notRepoMock.Object);

            var votoCEN = new VotoTorneoCEN(votoRepoMock.Object, propuestaRepoMock.Object, miembroRepoMock.Object, usuarioRepoMock.Object, participacionCEN, notificacionCEN);

            // Act: emitir 3 votos positivos
            votoCEN.EmitirVoto(propuesta.IdPropuesta, u1.IdUsuario, true);
            votoCEN.EmitirVoto(propuesta.IdPropuesta, u2.IdUsuario, true);
            votoCEN.EmitirVoto(propuesta.IdPropuesta, u3.IdUsuario, true);

            // Assert: participacion creada (participacionRepo.New called once)
            participacionRepoMock.Verify(r => r.New(It.IsAny<EN.ParticipacionTorneo>()), Times.Once);

            // Assert: notificaciones enviadas a los 3 miembros
            notRepoMock.Verify(r => r.New(It.IsAny<EN.Notificacion>()), Times.AtLeast(3));
        }
    }
}

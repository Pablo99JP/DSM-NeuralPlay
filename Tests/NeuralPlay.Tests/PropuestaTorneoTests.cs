using System.Linq;
using Moq;
using Xunit;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Tests
{
    public class PropuestaTorneoTests
    {
        [Fact]
        public void SiTodosVotanSi_SeCreaParticipacion_Y_SeNotifica()
        {
            // Arrange
            var propuesta = new PropuestaTorneo { IdPropuesta = 1, Votos = new System.Collections.Generic.List<VotoTorneo>() };
            var torneo = new Torneo { IdTorneo = 10, Nombre = "Copa Invierno LoL" };
            propuesta.Torneo = torneo;
            var equipo = new Equipo { IdEquipo = 100, Nombre = "Tigres Arkham" };
            propuesta.EquipoProponente = equipo;

            var u1 = new Usuario { IdUsuario = 201, Nick = "user1" };
            var u2 = new Usuario { IdUsuario = 202, Nick = "user2" };
            var u3 = new Usuario { IdUsuario = 203, Nick = "user3" };

            var miembroRepoMock = new Mock<IMiembroEquipoRepository>();
            miembroRepoMock.Setup(m => m.GetUsuariosByEquipo(equipo.IdEquipo)).Returns(new[] { u1, u2, u3 });

            var propuestaRepoMock = new Mock<IRepository<PropuestaTorneo>>();
            propuestaRepoMock.Setup(p => p.ReadById(propuesta.IdPropuesta)).Returns(propuesta);

            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            usuarioRepoMock.Setup(u => u.ReadById(u1.IdUsuario)).Returns(u1);
            usuarioRepoMock.Setup(u => u.ReadById(u2.IdUsuario)).Returns(u2);
            usuarioRepoMock.Setup(u => u.ReadById(u3.IdUsuario)).Returns(u3);

            var votoRepoMock = new Mock<IRepository<VotoTorneo>>();
            // When New is called, add the vote to propuesta.Votos to simulate persistence
            votoRepoMock.Setup(v => v.New(It.IsAny<VotoTorneo>())).Callback<VotoTorneo>(v => propuesta.Votos.Add(v));

            var participacionRepoMock = new Mock<ApplicationCore.Domain.Repositories.IParticipacionTorneoRepository>();
            var participacionCEN = new ParticipacionTorneoCEN(participacionRepoMock.Object);

            var notRepoMock = new Mock<IRepository<Notificacion>>();
            var notificacionCEN = new NotificacionCEN(notRepoMock.Object);

            var unitOfWorkMock = new Mock<ApplicationCore.Domain.Repositories.IUnitOfWork>();
            var votoCEN = new VotoTorneoCEN(votoRepoMock.Object, propuestaRepoMock.Object, miembroRepoMock.Object, usuarioRepoMock.Object, participacionCEN, notificacionCEN, unitOfWorkMock.Object);

            // Act: emitir 3 votos positivos
            votoCEN.EmitirVoto(propuesta.IdPropuesta, u1.IdUsuario, true);
            votoCEN.EmitirVoto(propuesta.IdPropuesta, u2.IdUsuario, true);
            votoCEN.EmitirVoto(propuesta.IdPropuesta, u3.IdUsuario, true);

            // Assert: participacion creada (participacionRepo.New called once)
            participacionRepoMock.Verify(r => r.New(It.IsAny<ParticipacionTorneo>()), Times.Once);

            // Assert: notificaciones enviadas to members
            notRepoMock.Verify(r => r.New(It.IsAny<Notificacion>()), Times.AtLeast(3));
        }
    }
}

using System;
using Xunit;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class CENsUnitTests
    {
        [Fact]
        public void Authentication_Login_Succeeds_With_Correct_Password()
        {
            var usuarioRepo = new InMemoryUsuarioRepository();
            var usuarioCEN = new UsuarioCEN(usuarioRepo);
            var authCEN = new AuthenticationCEN(usuarioRepo);

            var password = "secret123";
            var hashed = PasswordHasher.Hash(password);

            var user = usuarioCEN.NewUsuario("unituser", "unit@example.com", hashed);
            Assert.NotNull(user);

            var logged = authCEN.Login("unituser", password);
            Assert.NotNull(logged);
            Assert.Equal(user.IdUsuario, logged!.IdUsuario);
        }

        [Fact]
        public void PropuestaTorneo_AprobarSiVotosUnanimes_Works_For_Unanimous_True()
        {
            var propuestaRepo = new InMemoryRepository<PropuestaTorneo>();
            var participacionRepo = new InMemoryRepository<ParticipacionTorneo>();
            var propuestaCEN = new PropuestaTorneoCEN(propuestaRepo);

            var eq = new Equipo { Nombre = "EqUnit", FechaCreacion = DateTime.UtcNow };
            var torneo = new Torneo { Nombre = "TUnit", FechaInicio = DateTime.UtcNow, Estado = "Open" };
            var u = new Usuario { Nick = "u1", CorreoElectronico = "u1@example.com", ContrasenaHash = PasswordHasher.Hash("p") };

            var prop = propuestaCEN.NewPropuestaTorneo(eq, torneo, u);
            prop.Votos.Add(new VotoTorneo { Valor = true, FechaVoto = DateTime.UtcNow, Votante = u });
            prop.Votos.Add(new VotoTorneo { Valor = true, FechaVoto = DateTime.UtcNow, Votante = new Usuario { Nick = "u2", CorreoElectronico = "u2@example.com", ContrasenaHash = PasswordHasher.Hash("p") } });

            var result = propuestaCEN.AprobarSiVotosUnanimes(prop);
            Assert.True(result);
        }
    }
}

using System.Linq;
using Xunit;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.EN;

namespace Domain.UnitTests
{
    public class RepositorySpecializedTests
    {
        [Fact]
        public void GetEquiposByTorneo_ReturnsDistinctEquipos()
        {
            var repo = new InMemoryParticipacionTorneoRepository();

            var equipo1 = new Equipo { IdEquipo = 1, Nombre = "E1" };
            var equipo2 = new Equipo { IdEquipo = 2, Nombre = "E2" };
            var torneo1 = new Torneo { IdTorneo = 100, Nombre = "T1" };
            var torneo2 = new Torneo { IdTorneo = 200, Nombre = "T2" };

            // participations: two entries for equipo1 in torneo1 (duplicate), one for equipo2 in torneo1, one for equipo2 in torneo2
            var p1 = new ParticipacionTorneo { Equipo = equipo1, Torneo = torneo1 };
            var p2 = new ParticipacionTorneo { Equipo = equipo1, Torneo = torneo1 };
            var p3 = new ParticipacionTorneo { Equipo = equipo2, Torneo = torneo1 };
            var p4 = new ParticipacionTorneo { Equipo = equipo2, Torneo = torneo2 };

            repo.New(p1);
            repo.New(p2);
            repo.New(p3);
            repo.New(p4);

            var equiposT1 = repo.GetEquiposByTorneo(torneo1.IdTorneo).ToList();

            Assert.Contains(equiposT1, e => e.IdEquipo == equipo1.IdEquipo);
            Assert.Contains(equiposT1, e => e.IdEquipo == equipo2.IdEquipo);
            Assert.Equal(2, equiposT1.Count);
        }

        [Fact]
        public void GetTorneosByEquipo_ReturnsDistinctTorneos()
        {
            var repo = new InMemoryParticipacionTorneoRepository();

            var equipo1 = new Equipo { IdEquipo = 1, Nombre = "E1" };
            var torneo1 = new Torneo { IdTorneo = 100, Nombre = "T1" };
            var torneo2 = new Torneo { IdTorneo = 200, Nombre = "T2" };

            var p1 = new ParticipacionTorneo { Equipo = equipo1, Torneo = torneo1 };
            var p2 = new ParticipacionTorneo { Equipo = equipo1, Torneo = torneo1 };
            var p3 = new ParticipacionTorneo { Equipo = equipo1, Torneo = torneo2 };

            repo.New(p1);
            repo.New(p2);
            repo.New(p3);

            var torneos = repo.GetTorneosByEquipo(equipo1.IdEquipo).ToList();

            Assert.Contains(torneos, t => t.IdTorneo == torneo1.IdTorneo);
            Assert.Contains(torneos, t => t.IdTorneo == torneo2.IdTorneo);
            Assert.Equal(2, torneos.Count);
        }

        [Fact]
        public void GetUsuariosByEquipo_ReturnsUsuariosInEquipo()
        {
            var repo = new InMemoryMiembroEquipoRepository();

            var usuario1 = new Usuario { IdUsuario = 10, Nick = "alice" };
            var usuario2 = new Usuario { IdUsuario = 11, Nick = "bob" };
            var equipo = new Equipo { IdEquipo = 5, Nombre = "Team" };

            var m1 = new MiembroEquipo { Usuario = usuario1, Equipo = equipo };
            var m2 = new MiembroEquipo { Usuario = usuario2, Equipo = equipo };

            repo.New(m1);
            repo.New(m2);

            var usuarios = repo.GetUsuariosByEquipo(equipo.IdEquipo).ToList();

            Assert.Contains(usuarios, u => u.IdUsuario == usuario1.IdUsuario);
            Assert.Contains(usuarios, u => u.IdUsuario == usuario2.IdUsuario);
            Assert.Equal(2, usuarios.Count);
        }

        [Fact]
        public void GetUsuariosByComunidad_ReturnsUsuariosInComunidad()
        {
            var repo = new InMemoryMiembroComunidadRepository();

            var usuario1 = new Usuario { IdUsuario = 20, Nick = "carol" };
            var usuario2 = new Usuario { IdUsuario = 21, Nick = "dave" };
            var comunidad = new Comunidad { IdComunidad = 7, Nombre = "ComunidadX" };

            var m1 = new MiembroComunidad { Usuario = usuario1, Comunidad = comunidad };
            var m2 = new MiembroComunidad { Usuario = usuario2, Comunidad = comunidad };

            repo.New(m1);
            repo.New(m2);

            var usuarios = repo.GetUsuariosByComunidad(comunidad.IdComunidad).ToList();

            Assert.Contains(usuarios, u => u.IdUsuario == usuario1.IdUsuario);
            Assert.Contains(usuarios, u => u.IdUsuario == usuario2.IdUsuario);
            Assert.Equal(2, usuarios.Count);
        }
    }
}

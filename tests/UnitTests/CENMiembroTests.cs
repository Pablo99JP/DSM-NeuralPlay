using System;
using Xunit;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;

namespace Domain.UnitTests
{
    public class CENMiembroTests
    {
        [Fact]
        public void PromocionarAModerador_SetsRoleAndFechaAccion()
        {
            var repo = new InMemoryMiembroComunidadRepository();
            var cen = new MiembroComunidadCEN(repo);

            var user = new Usuario { IdUsuario = 1, Nick = "u1" };
            var com = new Comunidad { IdComunidad = 1, Nombre = "Com" };

            var miembro = cen.NewMiembroComunidad(user, com, ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO);
            Assert.Null(miembro.FechaAccion);

            cen.PromocionarAModerador(miembro);

            var stored = repo.ReadById(miembro.IdMiembroComunidad);
            Assert.Equal(ApplicationCore.Domain.Enums.RolComunidad.MODERADOR, stored!.Rol);
            Assert.NotNull(stored.FechaAccion);
        }

        [Fact]
        public void ActualizarFechaAccion_UpdatesTimestamp()
        {
            var repo = new InMemoryMiembroComunidadRepository();
            var cen = new MiembroComunidadCEN(repo);

            var user = new Usuario { IdUsuario = 2, Nick = "u2" };
            var com = new Comunidad { IdComunidad = 2, Nombre = "Com2" };

            var miembro = cen.NewMiembroComunidad(user, com, ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO);
            cen.ActualizarFechaAccion(miembro);

            var stored = repo.ReadById(miembro.IdMiembroComunidad);
            Assert.NotNull(stored!.FechaAccion);
        }

        [Fact]
        public void BanearMiembroEquipo_MarksExpulsadaAndSetsDates()
        {
            var repo = new InMemoryMiembroEquipoRepository();
            var cen = new MiembroEquipoCEN(repo);

            var user = new Usuario { IdUsuario = 3, Nick = "u3" };
            var eq = new Equipo { IdEquipo = 5, Nombre = "Team" };

            var miembro = cen.NewMiembroEquipo(user, eq, ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO);
            Assert.Null(miembro.FechaBaja);

            cen.BanearMiembroEquipo(miembro);

            var stored = repo.ReadById(miembro.IdMiembroEquipo);
            Assert.Equal(ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA, stored!.Estado);
            Assert.NotNull(stored.FechaBaja);
            Assert.NotNull(stored.FechaAccion);
        }
    }
}

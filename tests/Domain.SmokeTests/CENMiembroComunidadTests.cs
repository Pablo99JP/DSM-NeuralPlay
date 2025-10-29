using System;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Infrastructure.Memory;
using Xunit;

public class CENMiembroComunidadTests
{
    [Fact]
    public void NewMiembro_SetsFields_And_Salir_Expulsar_Work()
    {
        var repo = new InMemoryRepository<MiembroComunidad>();
        var cen = new MiembroComunidadCEN(repo);

    var user = new Usuario { IdUsuario = 1, Nick = "alice", CorreoElectronico = "a@x.com" };
        var com = new Comunidad { IdComunidad = 10, Nombre = "Gamers" };

        var m = cen.NewMiembroComunidad(user, com, ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO);

        Assert.NotNull(m);
        Assert.Equal(user, m.Usuario);
        Assert.Equal(com, m.Comunidad);
        Assert.Equal(ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, m.Estado);

        cen.Salir(m);
        Assert.Equal(ApplicationCore.Domain.Enums.EstadoMembresia.ABANDONADA, m.Estado);

        // Re-activate then expulsar
        m.Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA;
        cen.Expulsar(m);
        Assert.Equal(ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA, m.Estado);
    }
}

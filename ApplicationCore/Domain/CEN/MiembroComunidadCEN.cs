using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class MiembroComunidadCEN
    {
        private readonly IRepository<MiembroComunidad> _repo;

        public MiembroComunidadCEN(IRepository<MiembroComunidad> repo)
        {
            _repo = repo;
        }

        public MiembroComunidad NewMiembroComunidad(Usuario usuario, Comunidad comunidad, ApplicationCore.Domain.Enums.RolComunidad rol)
        {
            var m = new MiembroComunidad { Usuario = usuario, Comunidad = comunidad, Rol = rol, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = System.DateTime.UtcNow };
            _repo.New(m);
            return m;
        }

        public MiembroComunidad? ReadOID_MiembroComunidad(long id) => _repo.ReadById(id);
        public System.Collections.Generic.IEnumerable<MiembroComunidad> ReadAll_MiembroComunidad() => _repo.ReadAll();
        public void ModifyMiembroComunidad(MiembroComunidad m) => _repo.Modify(m);
        public void DestroyMiembroComunidad(long id) => _repo.Destroy(id);

        // Custom operations
        public void Salir(MiembroComunidad miembro)
        {
            miembro.Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ABANDONADA;
            miembro.FechaBaja = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }

        public void Expulsar(MiembroComunidad miembro)
        {
            miembro.Estado = ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA;
            miembro.FechaBaja = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }

        public void CambiarRol(MiembroComunidad miembro, ApplicationCore.Domain.Enums.RolComunidad nuevoRol)
        {
            miembro.Rol = nuevoRol;
            _repo.Modify(miembro);
        }
    }
}

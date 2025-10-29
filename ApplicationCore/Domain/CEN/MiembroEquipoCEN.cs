using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class MiembroEquipoCEN
    {
        private readonly IRepository<MiembroEquipo> _repo;

        public MiembroEquipoCEN(IRepository<MiembroEquipo> repo)
        {
            _repo = repo;
        }

        public MiembroEquipo NewMiembroEquipo(Usuario usuario, Equipo equipo, ApplicationCore.Domain.Enums.RolEquipo rol)
        {
            var m = new MiembroEquipo { Usuario = usuario, Equipo = equipo, Rol = rol, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = System.DateTime.UtcNow };
            _repo.New(m);
            return m;
        }

        public MiembroEquipo? ReadOID_MiembroEquipo(long id) => _repo.ReadById(id);
        public IEnumerable<MiembroEquipo> ReadAll_MiembroEquipo() => _repo.ReadAll();
        public void ModifyMiembroEquipo(MiembroEquipo m) => _repo.Modify(m);
        public void DestroyMiembroEquipo(long id) => _repo.Destroy(id);
        public IEnumerable<MiembroEquipo> ReadFilter_MiembroEquipo(string filter) => _repo.ReadFilter(filter);
    }
}

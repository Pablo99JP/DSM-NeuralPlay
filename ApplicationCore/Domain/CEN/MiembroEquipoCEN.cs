using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class MiembroEquipoCEN
    {
        private readonly IMiembroEquipoRepository _repo;

        public MiembroEquipoCEN(IMiembroEquipoRepository repo)
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
        public IEnumerable<MiembroEquipo> BuscarMiembrosEquipoPorNickUsuario(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarMiembrosEquipoPorNickUsuario");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<MiembroEquipo> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }

        // ReadFilter custom: Selecciona todos los Usuarios que tengan una membresía de equipo cuyo equipo coincida con el id
        public System.Collections.Generic.IEnumerable<ApplicationCore.Domain.EN.Usuario> ReadFilter_UsuariosByEquipoMembership(long idEquipo)
        {
            // Delegate to repository for efficiency
            return _repo.GetUsuariosByEquipo(idEquipo);
        }

        // Banear miembro de equipo: marcar como expulsado y fijar fecha de baja/acción
        public void BanearMiembroEquipo(MiembroEquipo miembro)
        {
            miembro.Estado = ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA;
            miembro.FechaBaja = System.DateTime.UtcNow;
            miembro.FechaAccion = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }

        // Actualizar la fecha de accion
        public void ActualizarFechaAccion(MiembroEquipo miembro)
        {
            miembro.FechaAccion = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }
    }
}

using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.CEN
{
    public class SolicitudIngresoCEN
    {
        private readonly IRepository<SolicitudIngreso> _repo;
        private readonly IMiembroEquipoRepository _miembroEquipoRepo;

        public SolicitudIngresoCEN(IRepository<SolicitudIngreso> repo, IMiembroEquipoRepository miembroEquipoRepo)
        {
            _repo = repo;
            _miembroEquipoRepo = miembroEquipoRepo;
        }

        public SolicitudIngreso NewSolicitudIngreso(TipoInvitacion tipo, Usuario solicitante, Comunidad? comunidad = null, Equipo? equipo = null)
        {
            // Si se solicita ingreso a una comunidad, no se permite si el usuario ya forma parte de un equipo de esa comunidad
            if (comunidad != null)
            {
                var alreadyInTeamOfCommunity = _miembroEquipoRepo.ReadAll()
                    .Any(m => m.Usuario != null && m.Usuario.IdUsuario == solicitante.IdUsuario && m.Equipo != null && m.Equipo.Comunidad != null && m.Equipo.Comunidad.IdComunidad == comunidad.IdComunidad && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);
                if (alreadyInTeamOfCommunity)
                {
                    throw new System.InvalidOperationException("El usuario ya forma parte de un equipo de esa comunidad y no puede solicitar ingreso a la comunidad.");
                }
            }

            var s = new SolicitudIngreso { Tipo = tipo, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE, FechaSolicitud = System.DateTime.UtcNow, Solicitante = solicitante, Comunidad = comunidad, Equipo = equipo };
            _repo.New(s);
            return s;
        }

        public SolicitudIngreso? ReadOID_SolicitudIngreso(long id) => _repo.ReadById(id);
        public IEnumerable<SolicitudIngreso> ReadAll_SolicitudIngreso() => _repo.ReadAll();
        public void ModifySolicitudIngreso(SolicitudIngreso s) => _repo.Modify(s);
        public void DestroySolicitudIngreso(long id) => _repo.Destroy(id);
        public IEnumerable<SolicitudIngreso> BuscarSolicitudesIngresoPorNickSolicitante(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarSolicitudesIngresoPorNickSolicitante");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<SolicitudIngreso> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}

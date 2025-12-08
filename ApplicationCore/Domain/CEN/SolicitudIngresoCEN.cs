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
        private readonly IMiembroComunidadRepository _miembroComunidadRepo;

        public SolicitudIngresoCEN(IRepository<SolicitudIngreso> repo, IMiembroEquipoRepository miembroEquipoRepo, IMiembroComunidadRepository miembroComunidadRepo)
        {
            _repo = repo;
            _miembroEquipoRepo = miembroEquipoRepo;
            _miembroComunidadRepo = miembroComunidadRepo;
        }

        public SolicitudIngreso NewSolicitudIngreso(TipoInvitacion tipo, Usuario solicitante, Comunidad? comunidad = null, Equipo? equipo = null)
        {
            // Validar según el tipo de solicitud
            if (comunidad != null)
            {
                // Verificar si el usuario ya es miembro de la comunidad
                var alreadyInCommunity = _miembroComunidadRepo.ReadAll()
                    .Any(m => m.Usuario != null && m.Usuario.IdUsuario == solicitante.IdUsuario 
                           && m.Comunidad != null && m.Comunidad.IdComunidad == comunidad.IdComunidad 
                           && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);
                if (alreadyInCommunity)
                {
                    throw new System.InvalidOperationException("El usuario ya es miembro de esta comunidad.");
                }
            }

            if (equipo != null)
            {
                // Verificar si el usuario ya es miembro del equipo
                var alreadyInTeam = _miembroEquipoRepo.ReadAll()
                    .Any(m => m.Usuario != null && m.Usuario.IdUsuario == solicitante.IdUsuario 
                           && m.Equipo != null && m.Equipo.IdEquipo == equipo.IdEquipo 
                           && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);
                if (alreadyInTeam)
                {
                    throw new System.InvalidOperationException("El usuario ya es miembro de este equipo.");
                }
            }

            // Las solicitudes de comunidad se aprueban automáticamente (comunidades públicas)
            // Las solicitudes de equipo quedan pendientes
            var estado = tipo == TipoInvitacion.COMUNIDAD 
                ? ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA 
                : ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE;
            
            var fechaResolucion = tipo == TipoInvitacion.COMUNIDAD 
                ? System.DateTime.UtcNow 
                : (System.DateTime?)null;

            var s = new SolicitudIngreso 
            { 
                Tipo = tipo, 
                Estado = estado, 
                FechaSolicitud = System.DateTime.UtcNow, 
                FechaResolucion = fechaResolucion,
                Solicitante = solicitante, 
                Comunidad = comunidad, 
                Equipo = equipo 
            };
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

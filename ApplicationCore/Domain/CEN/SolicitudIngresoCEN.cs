using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.CEN
{
    public class SolicitudIngresoCEN
    {
        private readonly IRepository<SolicitudIngreso> _repo;

        public SolicitudIngresoCEN(IRepository<SolicitudIngreso> repo)
        {
            _repo = repo;
        }

        public SolicitudIngreso NewSolicitudIngreso(TipoInvitacion tipo, Usuario solicitante, Comunidad? comunidad = null, Equipo? equipo = null)
        {
            var s = new SolicitudIngreso { Tipo = tipo, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE, FechaSolicitud = System.DateTime.UtcNow, Solicitante = solicitante, Comunidad = comunidad, Equipo = equipo };
            _repo.New(s);
            return s;
        }

        public SolicitudIngreso? ReadOID_SolicitudIngreso(long id) => _repo.ReadById(id);
        public IEnumerable<SolicitudIngreso> ReadAll_SolicitudIngreso() => _repo.ReadAll();
        public void ModifySolicitudIngreso(SolicitudIngreso s) => _repo.Modify(s);
        public void DestroySolicitudIngreso(long id) => _repo.Destroy(id);
        public IEnumerable<SolicitudIngreso> ReadFilter_SolicitudIngreso(string filter) => _repo.ReadFilter(filter);
    }
}

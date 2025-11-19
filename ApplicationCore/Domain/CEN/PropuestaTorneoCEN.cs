using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class PropuestaTorneoCEN
    {
        private readonly IRepository<PropuestaTorneo> _repo;

        public PropuestaTorneoCEN(IRepository<PropuestaTorneo> repo)
        {
            _repo = repo;
        }

        public PropuestaTorneo NewPropuestaTorneo(Equipo equipo, Torneo torneo, Usuario propuestoPor)
        {
            var p = new PropuestaTorneo { EquipoProponente = equipo, Torneo = torneo, PropuestoPor = propuestoPor, FechaPropuesta = System.DateTime.UtcNow, Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE };
            _repo.New(p);
            return p;
        }

        public PropuestaTorneo? ReadOID_PropuestaTorneo(long id) => _repo.ReadById(id);
        public IEnumerable<PropuestaTorneo> ReadAll_PropuestaTorneo() => _repo.ReadAll();
        public void ModifyPropuestaTorneo(PropuestaTorneo p) => _repo.Modify(p);
        public void DestroyPropuestaTorneo(long id) => _repo.Destroy(id);
        public IEnumerable<PropuestaTorneo> BuscarPropuestasTorneoPorNombreTorneo(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarPropuestasTorneoPorNombreTorneo");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<PropuestaTorneo> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }

        // Custom: aprobar si votos unanimes
        public bool AprobarSiVotosUnanimes(PropuestaTorneo propuesta)
        {
            if (propuesta.Votos == null || !propuesta.Votos.Any()) return false;
            var todosSi = propuesta.Votos.All(v => v.Valor);
            if (todosSi)
            {
                propuesta.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA;
                _repo.Modify(propuesta);
                return true;
            }
            return false;
        }
    }
}

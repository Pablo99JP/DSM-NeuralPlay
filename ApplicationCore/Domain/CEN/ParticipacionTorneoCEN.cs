using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ParticipacionTorneoCEN
    {
        private readonly IParticipacionTorneoRepository _repo;
        private readonly IRepository<Torneo> _torneoRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ParticipacionTorneoCEN(IParticipacionTorneoRepository repo, IRepository<Torneo> torneoRepo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _torneoRepo = torneoRepo;
            _unitOfWork = unitOfWork;
        }

        public ParticipacionTorneo NewParticipacionTorneo(Equipo equipo, Torneo torneo)
        {
            var p = new ParticipacionTorneo { Equipo = equipo, Torneo = torneo, Estado = ApplicationCore.Domain.Enums.EstadoParticipacion.PENDIENTE.ToString(), FechaAlta = System.DateTime.UtcNow };
            _repo.New(p);
            return p;
        }

        public ParticipacionTorneo? ReadOID_ParticipacionTorneo(long id) => _repo.ReadById(id);
        public IEnumerable<ParticipacionTorneo> ReadAll_ParticipacionTorneo() => _repo.ReadAll();
        
        public void ModifyParticipacionTorneo(ParticipacionTorneo p)
        {
            _repo.Modify(p);
            
            // Si la participación se acaba de aceptar, validar si el torneo debe abrirse
            if (p.Torneo != null && string.Equals(p.Estado, "ACEPTADA", System.StringComparison.OrdinalIgnoreCase))
            {
                _unitOfWork.SaveChanges();
                ValidarYAbrirTorneo(p.Torneo.IdTorneo);
            }
        }
        
        // Método interno para validar y abrir torneo
        private void ValidarYAbrirTorneo(long idTorneo)
        {
            var torneo = _torneoRepo.ReadById(idTorneo);
            if (torneo == null) return;

            // Solo se puede abrir si está en estado PENDIENTE
            if (!string.Equals(torneo.Estado, "PENDIENTE", System.StringComparison.OrdinalIgnoreCase))
                return;

            // Contar participaciones aceptadas
            var participacionesAceptadas = _repo.ReadAll()
                .Count(p => p.Torneo != null && 
                           p.Torneo.IdTorneo == idTorneo && 
                           string.Equals(p.Estado, "ACEPTADA", System.StringComparison.OrdinalIgnoreCase));

            if (participacionesAceptadas >= 2)
            {
                torneo.Estado = "ABIERTO";
                _torneoRepo.Modify(torneo);
                _unitOfWork.SaveChanges();
            }
        }
        public void DestroyParticipacionTorneo(long id) => _repo.Destroy(id);
        public IEnumerable<ParticipacionTorneo> BuscarParticipacionesTorneoPorEstado(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarParticipacionesTorneoPorEstado");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<ParticipacionTorneo> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }

        // ReadFilters custom (delegados a repositorio para ejecución eficiente en BD)
        public System.Collections.Generic.IEnumerable<ApplicationCore.Domain.EN.Equipo> ReadFilter_EquiposByTorneo(long idTorneo)
            => _repo.GetEquiposByTorneo(idTorneo);

        public System.Collections.Generic.IEnumerable<ApplicationCore.Domain.EN.Torneo> ReadFilter_TorneosByEquipo(long idEquipo)
            => _repo.GetTorneosByEquipo(idEquipo);
    }
}

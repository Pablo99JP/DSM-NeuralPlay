using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class TorneoCEN
    {
        private readonly IRepository<Torneo> _repo;
        private readonly IRepository<ParticipacionTorneo> _participacionRepo;
        private readonly IRepository<PropuestaTorneo> _propuestaRepo;
        private readonly IUnitOfWork _unitOfWork;

        public TorneoCEN(IRepository<Torneo> repo, IRepository<ParticipacionTorneo> participacionRepo, IRepository<PropuestaTorneo> propuestaRepo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _participacionRepo = participacionRepo;
            _propuestaRepo = propuestaRepo;
            _unitOfWork = unitOfWork;
        }

        public Torneo NewTorneo(string nombre, System.DateTime fechaInicio, string? reglas, Comunidad? comunidadOrganizadora, Usuario? creador)
        {
            var t = new Torneo
            {
                Nombre = nombre,
                FechaInicio = fechaInicio,
                Reglas = reglas,
                Estado = "PENDIENTE",
                ComunidadOrganizadora = comunidadOrganizadora,
                Creador = creador
            };
            _repo.New(t);
            return t;
        }

        public Torneo? ReadOID_Torneo(long id) => _repo.ReadById(id);
        
        public IEnumerable<Torneo> ReadAll_Torneo() => _repo.ReadAll();
        
        public void ModifyTorneo(Torneo t) => _repo.Modify(t);
        
        public void DestroyTorneo(long id)
        {
            // Eliminar primero todas las participaciones asociadas
            var participaciones = _participacionRepo.ReadAll()
                .Where(p => p.Torneo != null && p.Torneo.IdTorneo == id)
                .ToList();
            
            foreach (var p in participaciones)
            {
                _participacionRepo.Destroy(p.IdParticipacion);
            }

            // Eliminar todas las propuestas asociadas
            var propuestas = _propuestaRepo.ReadAll()
                .Where(p => p.Torneo != null && p.Torneo.IdTorneo == id)
                .ToList();
            
            foreach (var p in propuestas)
            {
                _propuestaRepo.Destroy(p.IdPropuesta);
            }

            // Finalmente eliminar el torneo
            _repo.Destroy(id);
            _unitOfWork.SaveChanges();
        }
        
        public IEnumerable<Torneo> BuscarTorneosPorNombre(string filtro) => _repo.ReadFilter(filtro);

        // Método para validar y cambiar el estado de un torneo de PENDIENTE a ABIERTO
        // si tiene al menos 2 participaciones aceptadas
        public bool ValidarYAbrirTorneo(long idTorneo)
        {
            var torneo = _repo.ReadById(idTorneo);
            if (torneo == null) return false;

            // Solo se puede abrir si está en estado PENDIENTE
            if (!string.Equals(torneo.Estado, "PENDIENTE", System.StringComparison.OrdinalIgnoreCase))
                return false;

            // Contar participaciones aceptadas o propuestas aceptadas
            var participacionesAceptadas = _participacionRepo.ReadAll()
                .Count(p => p.Torneo != null && 
                           p.Torneo.IdTorneo == idTorneo && 
                           string.Equals(p.Estado, "ACEPTADA", System.StringComparison.OrdinalIgnoreCase));

            if (participacionesAceptadas >= 2)
            {
                torneo.Estado = "ABIERTO";
                _repo.Modify(torneo);
                _unitOfWork.SaveChanges();
                return true;
            }

            return false;
        }
    }
}

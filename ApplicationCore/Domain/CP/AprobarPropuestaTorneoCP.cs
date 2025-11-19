using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    public class AprobarPropuestaTorneoCP
    {
        private readonly IRepository<PropuestaTorneo> _propRepo;
        private readonly IRepository<ParticipacionTorneo> _partRepo;
        private readonly IUnitOfWork _uow;

        public AprobarPropuestaTorneoCP(IRepository<PropuestaTorneo> propRepo, IRepository<ParticipacionTorneo> partRepo, IUnitOfWork uow)
        {
            _propRepo = propRepo;
            _partRepo = partRepo;
            _uow = uow;
        }

        public bool Ejecutar(PropuestaTorneo propuesta)
        {
            // Si todos los votos son "si", unir equipo al torneo
            if (propuesta.Votos == null || propuesta.Votos.Count == 0) return false;
            var todosSi = true;
            foreach (var v in propuesta.Votos)
            {
                if (!v.Valor) { todosSi = false; break; }
            }
            if (!todosSi) return false;

            var participacion = new ParticipacionTorneo { Equipo = propuesta.EquipoProponente ?? throw new System.Exception("Equipo nulo"), Torneo = propuesta.Torneo ?? throw new System.Exception("Torneo nulo"), Estado = ApplicationCore.Domain.Enums.EstadoParticipacion.ACEPTADA.ToString(), FechaAlta = System.DateTime.UtcNow };
            _partRepo.New(participacion);
            propuesta.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA;
            _propRepo.Modify(propuesta);
            _uow.SaveChanges();
            return true;
        }
    }
}

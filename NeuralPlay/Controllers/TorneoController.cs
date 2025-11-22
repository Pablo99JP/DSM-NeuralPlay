using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using System.Linq;

namespace NeuralPlay.Controllers
{
    public class TorneoController : Controller
    {
        private readonly ApplicationCore.Domain.Repositories.IRepository<Torneo> _torneoRepo;
        private readonly PropuestaTorneoCEN _propuestaCEN;

        public TorneoController(ApplicationCore.Domain.Repositories.IRepository<Torneo> torneoRepo, PropuestaTorneoCEN propuestaCEN)
        {
            _torneoRepo = torneoRepo;
            _propuestaCEN = propuestaCEN;
        }

        public IActionResult Index()
        {
            var torneos = _torneoRepo.ReadAll().ToList();
            return View(torneos);
        }

        public IActionResult Details(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();
            return View(torneo);
        }

        // Accion simplificada para mostrar el formulario de proponer participación
        [HttpGet]
        public IActionResult ProponerParticipacion(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();
            // La vista / lógica real comprobará si el usuario es líder de equipo
            return View(torneo);
        }
    }
}

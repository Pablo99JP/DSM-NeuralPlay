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
        private readonly NeuralPlay.Services.IUsuarioAuth _usuarioAuth;
        private readonly ApplicationCore.Domain.CEN.MiembroEquipoCEN _miembroEquipoCEN;
        private readonly ApplicationCore.Domain.CEN.EquipoCEN _equipoCEN;
        private readonly ApplicationCore.Domain.CEN.ParticipacionTorneoCEN _participacionTorneoCEN;

        public TorneoController(ApplicationCore.Domain.Repositories.IRepository<Torneo> torneoRepo,
            PropuestaTorneoCEN propuestaCEN,
            NeuralPlay.Services.IUsuarioAuth usuarioAuth,
            ApplicationCore.Domain.CEN.MiembroEquipoCEN miembroEquipoCEN,
            ApplicationCore.Domain.CEN.EquipoCEN equipoCEN,
            ApplicationCore.Domain.CEN.ParticipacionTorneoCEN participacionTorneoCEN)
        {
            _torneoRepo = torneoRepo;
            _propuestaCEN = propuestaCEN;
            _usuarioAuth = usuarioAuth;
            _miembroEquipoCEN = miembroEquipoCEN;
            _equipoCEN = equipoCEN;
            _participacionTorneoCEN = participacionTorneoCEN;
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

            var vm = new NeuralPlay.Models.TorneoDetailsViewModel { Torneo = torneo };

            // Load propuestas and participaciones explicitly because Torneo mapping does not include collections
            var propuestas = _propuestaCEN.ReadAll_PropuestaTorneo().Where(p => p.Torneo != null && p.Torneo.IdTorneo == id).ToList();
            var participaciones = _participacionTorneoCEN.ReadAll_ParticipacionTorneo().Where(p => p.Torneo != null && p.Torneo.IdTorneo == id).ToList();

            vm.Propuestas = propuestas;
            vm.Participaciones = participaciones;

            return View(vm);
        }

        // Accion simplificada para mostrar el formulario de proponer participación
        [HttpGet]
        public IActionResult ProponerParticipacion(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var vm = new NeuralPlay.Models.ProponerParticipacionViewModel { Torneo = torneo };

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                vm.Message = "Debes iniciar sesión para proponer participación.";
                return View(vm);
            }

            // Obtener los miembros de equipo para el usuario (buscar por nick)
            var miembros = _miembroEquipoCEN.BuscarMiembrosEquipoPorNickUsuario(usuario.Nick);
            var equipos = new List<Equipo>();
            foreach (var m in miembros)
            {
                if (m.Equipo != null) equipos.Add(m.Equipo);
            }

            vm.EquiposDisponibles = equipos;
            // Show any TempData messages from previous POST redirect
            if (TempData.ContainsKey("ErrorMessage")) vm.Message = TempData["ErrorMessage"]?.ToString();
            if (TempData.ContainsKey("SuccessMessage")) vm.Message = TempData["SuccessMessage"]?.ToString();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProponerParticipacion(long id, NeuralPlay.Models.ProponerParticipacionViewModel model)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null) return Forbid();

            // Validar equipo
            var equipo = _equipoCEN.ReadOID_Equipo(model.SelectedEquipoId);
            if (equipo == null) return BadRequest("Equipo no válido");

            // 1) Validar que el torneo esté abierto
            if (!string.Equals(torneo.Estado, "ABIERTO", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "No se pueden crear propuestas en un torneo que no está abierto.";
                return RedirectToAction(nameof(ProponerParticipacion), new { id = torneo.IdTorneo });
            }

            // 2) Validar que el usuario pertenezca al equipo y tenga rol ADMIN
            var miembrosUsuario = _miembroEquipoCEN.BuscarMiembrosEquipoPorNickUsuario(usuario.Nick);
            var miembroProp = miembrosUsuario.FirstOrDefault(m => m.Equipo != null && m.Equipo.IdEquipo == equipo.IdEquipo);
            if (miembroProp == null || miembroProp.Rol != ApplicationCore.Domain.Enums.RolEquipo.ADMIN || miembroProp.Estado != ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
            {
                TempData["ErrorMessage"] = "Solo administradores de equipo activos pueden proponer participación.";
                return RedirectToAction(nameof(ProponerParticipacion), new { id = torneo.IdTorneo });
            }

            // 3) Evitar propuestas duplicadas (mismo equipo + mismo torneo)
            var exists = _propuestaCEN.ReadAll_PropuestaTorneo().Any(p => p.Torneo != null && p.EquipoProponente != null && p.Torneo.IdTorneo == torneo.IdTorneo && p.EquipoProponente.IdEquipo == equipo.IdEquipo);
            if (exists)
            {
                TempData["ErrorMessage"] = "Ya existe una propuesta de este equipo para este torneo.";
                return RedirectToAction(nameof(ProponerParticipacion), new { id = torneo.IdTorneo });
            }

            // Crear propuesta vía CEN
            var propuesta = _propuestaCEN.NewPropuestaTorneo(equipo, torneo, usuario);
            TempData["SuccessMessage"] = "Propuesta creada correctamente.";

            // Redirigir a detalles del torneo
            return RedirectToAction("Details", new { id = torneo.IdTorneo });
        }
    }
}

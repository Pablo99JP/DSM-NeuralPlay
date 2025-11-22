using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NeuralPlay.Models;
using System.Linq;

namespace NeuralPlay.Controllers
{
    public class PropuestaTorneoController : Controller
    {
        private readonly PropuestaTorneoCEN _propuestaCEN;
        private readonly VotoTorneoCEN _votoCEN;
        private readonly IMiembroEquipoRepository _miembroEquipoRepo;

        public PropuestaTorneoController(PropuestaTorneoCEN propuestaCEN, VotoTorneoCEN votoCEN, IMiembroEquipoRepository miembroEquipoRepo)
        {
            _propuestaCEN = propuestaCEN;
            _votoCEN = votoCEN;
            _miembroEquipoRepo = miembroEquipoRepo;
        }

        public IActionResult Index()
        {
            // Mostrar propuestas pendientes de mis equipos (este ejemplo devuelve todas pendientes)
            var todas = _propuestaCEN.ReadAll_PropuestaTorneo().Where(p => p.Estado == ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE);
            return View(todas.ToList());
        }

        public IActionResult Details(long id)
        {
            var p = _propuestaCEN.ReadOID_PropuestaTorneo(id);
            if (p == null) return NotFound();
            var miembros = p.EquipoProponente != null ? _miembroEquipoRepo.GetUsuariosByEquipo(p.EquipoProponente.IdEquipo).ToList() : new System.Collections.Generic.List<Usuario>();

            // Determinar si el usuario actual ya ha votado
            long? currentUserId = null;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var nameId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(nameId, out var parsed)) currentUserId = parsed;
            }
            else if (HttpContext.Session != null)
            {
                currentUserId = HttpContext.Session.GetInt32("UserId");
            }

            var model = new PropuestaTorneoDetailsViewModel { Propuesta = p, MiembrosEquipo = miembros };
            ViewData["CurrentUserId"] = currentUserId;
            return View(model);
        }

        [HttpPost]
        public IActionResult Votar(long propuestaId, bool decision)
        {
            // Usuario logueado simulado: extraer Id desde sesión o claims en implementación real
            // Obtener id de usuario desde claims (Identity) o sesión como fallback
            long? usuarioId = null;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var nameId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(nameId, out var parsed)) usuarioId = parsed;
            }
            if (usuarioId == null)
            {
                usuarioId = HttpContext.Session.GetInt32("UserId");
            }
            if (!usuarioId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión o configurar un UserId en la sesión para poder votar.";
                return RedirectToAction("Details", new { id = propuestaId });
            }

            _votoCEN.EmitirVoto(propuestaId, usuarioId.Value, decision);
            return RedirectToAction("Details", new { id = propuestaId });
        }
    }
}

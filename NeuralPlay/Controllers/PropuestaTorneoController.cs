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
            // Mostrar todas las propuestas con sus distintos estados para trazabilidad
            var propuestas = _propuestaCEN.ReadAll_PropuestaTorneo()
                .OrderByDescending(p => p.FechaPropuesta)
                .ToList();
            return View(propuestas);
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
                currentUserId = HttpContext.Session.GetInt32("UsuarioId");
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
                usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            }
            
            System.Console.WriteLine($"[DEBUG VOTAR] PropuestaId={propuestaId}, Decision={decision}, UsuarioId={usuarioId}");
            
            if (!usuarioId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para poder votar.";
                return RedirectToAction("Login", "Usuario");
            }

            try
            {
                _votoCEN.EmitirVoto(propuestaId, usuarioId.Value, decision);
                TempData["Success"] = "Voto registrado correctamente";
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[ERROR VOTAR] {ex.Message}");
                TempData["Error"] = $"Error al votar: {ex.Message}";
            }
            
            return RedirectToAction("Details", new { id = propuestaId });
        }
    }
}

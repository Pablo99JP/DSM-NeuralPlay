using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Controllers
{
    public class SolicitudIngresoController : BasicController
    {
        private readonly SolicitudIngresoCEN _solicitudCEN;

        public SolicitudIngresoController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository, SolicitudIngresoCEN solicitudCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _solicitudCEN = solicitudCEN;
        }

        // GET: /SolicitudIngreso
        public IActionResult Index()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var all = _solicitudCEN.ReadAll_SolicitudIngreso();
            var mine = all.Where(s => s.Solicitante != null && s.Solicitante.IdUsuario == userId.Value);
            var vm = SolicitudIngresoAssembler.ConvertListENToViewModel(mine);
            return View(vm);
        }

        // GET: /SolicitudIngreso/Create
        public IActionResult Create()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            return View();
        }

        // POST: /SolicitudIngreso/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SolicitudIngresoViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var solicitante = _usuarioCEN.ReadOID_Usuario(userId.Value);
            if (solicitante == null) return NotFound();

            Comunidad? comunidad = null;
            Equipo? equipo = null;
            if (model.ComunidadId != 0) comunidad = new Comunidad { IdComunidad = model.ComunidadId };
            if (model.EquipoId != 0) equipo = new Equipo { IdEquipo = model.EquipoId };

            _solicitudCEN.NewSolicitudIngreso(model.Tipo, solicitante, comunidad, equipo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /SolicitudIngreso/Details/5
        public IActionResult Details(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var s = _solicitudCEN.ReadOID_SolicitudIngreso(id);
            if (s == null) return NotFound();
            if (s.Solicitante == null || s.Solicitante.IdUsuario != userId.Value) return StatusCode(403);

            var vm = SolicitudIngresoAssembler.ConvertENToViewModel(s);
            return View(vm);
        }

        // GET: /SolicitudIngreso/Edit/5
        public IActionResult Edit(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var s = _solicitudCEN.ReadOID_SolicitudIngreso(id);
            if (s == null) return NotFound();
            if (s.Solicitante == null || s.Solicitante.IdUsuario != userId.Value) return StatusCode(403);

            var vm = SolicitudIngresoAssembler.ConvertENToViewModel(s);
            return View(vm);
        }

        // POST: /SolicitudIngreso/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SolicitudIngresoViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var s = _solicitudCEN.ReadOID_SolicitudIngreso(model.Id);
            if (s == null) return NotFound();
            if (s.Solicitante == null || s.Solicitante.IdUsuario != userId.Value) return StatusCode(403);

            s.Estado = model.Estado;
            s.FechaResolucion = model.FechaResolucion;
            _solicitudCEN.ModifySolicitudIngreso(s);

            return RedirectToAction(nameof(Index));
        }

        // POST: /SolicitudIngreso/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var s = _solicitudCEN.ReadOID_SolicitudIngreso(id);
            if (s == null) return NotFound();
            if (s.Solicitante == null || s.Solicitante.IdUsuario != userId.Value) return StatusCode(403);

            _solicitudCEN.DestroySolicitudIngreso(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

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
    public class NotificacionController : BasicController
    {
        private readonly NotificacionCEN _notificacionCEN;

        public NotificacionController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository, NotificacionCEN notificacionCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _notificacionCEN = notificacionCEN;
        }

        // GET: /Notificacion
        public IActionResult Index()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var all = _notificacionCEN.ReadAll_Notificacion();
            var mine = all.Where(n => n.Destinatario != null && n.Destinatario.IdUsuario == userId.Value);
            var vm = NotificacionAssembler.ConvertListENToModel(mine);
            return View(vm);
        }

        // GET: /Notificacion/Create
        public IActionResult Create()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            return View();
        }

        // POST: /Notificacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NotificacionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var usuario = _usuarioCEN.ReadOID_Usuario(userId.Value);
            if (usuario == null) return NotFound();

            // Crear notificacion tipo SISTEMA por defecto
            _notificacionCEN.NewNotificacion(TipoNotificacion.SISTEMA, model.Texto ?? string.Empty, usuario);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Notificacion/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var n = _notificacionCEN.ReadOID_Notificacion(id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return Forbid();

            _notificacionCEN.DestroyNotificacion(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Notificacion/Details/5
        public IActionResult Details(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var n = _notificacionCEN.ReadOID_Notificacion(id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return Forbid();

            var vm = NotificacionAssembler.ConvertENToViewModel(n);
            return View(vm);
        }

        // GET: /Notificacion/Edit/5
        public IActionResult Edit(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var n = _notificacionCEN.ReadOID_Notificacion(id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return Forbid();

            var vm = NotificacionAssembler.ConvertENToViewModel(n);
            return View(vm);
        }

        // POST: /Notificacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NotificacionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var n = _notificacionCEN.ReadOID_Notificacion(model.Id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return Forbid();

            // Update allowed fields
            n.Mensaje = model.Texto ?? string.Empty;
            n.Leida = model.Leido;
            _notificacionCEN.ModifyNotificacion(n);

            return RedirectToAction(nameof(Index));
        }
    }
}

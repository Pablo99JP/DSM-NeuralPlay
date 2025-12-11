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

        // GET: /Notificacion/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var n = _notificacionCEN.ReadOID_Notificacion(id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return StatusCode(403);

            var vm = NotificacionAssembler.ConvertENToViewModel(n);
            return View(vm);
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
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return StatusCode(403);

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
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return StatusCode(403);

            // Mark notification as read
            if (!n.Leida)
            {
                n.Leida = true;
                _notificacionCEN.ModifyNotificacion(n);
            }

            var vm = NotificacionAssembler.ConvertENToViewModel(n);
            return View(vm);
        }
    }
}

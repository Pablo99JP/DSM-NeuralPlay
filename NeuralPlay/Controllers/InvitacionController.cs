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
    public class InvitacionController : BasicController
    {
        private readonly InvitacionCEN _invitacionCEN;
        private readonly IUsuarioRepository _usuarioRepo;

        public InvitacionController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository, InvitacionCEN invitacionCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _invitacionCEN = invitacionCEN;
            _usuarioRepo = usuarioRepository;
        }

        // GET: /Invitacion
        public IActionResult Index()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var all = _invitacionCEN.ReadAll_Invitacion();
            var mine = all.Where(i => (i.Emisor != null && i.Emisor.IdUsuario == userId.Value) || (i.Destinatario != null && i.Destinatario.IdUsuario == userId.Value));
            var vm = InvitacionAssembler.ConvertListENToViewModel(mine);
            return View(vm);
        }

        // GET: /Invitacion/Create
        public IActionResult Create()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            return View();
        }

        // POST: /Invitacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InvitacionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var emisor = _usuarioCEN.ReadOID_Usuario(userId.Value);
            if (emisor == null) return NotFound();

            var destinatario = _usuarioRepo.ReadById(model.DestinatarioId);
            if (destinatario == null) return NotFound();

            Comunidad? comunidad = null;
            Equipo? equipo = null;
            if (model.ComunidadId != 0) comunidad = new Comunidad { IdComunidad = model.ComunidadId };
            if (model.EquipoId != 0) equipo = new Equipo { IdEquipo = model.EquipoId };

            _invitacionCEN.NewInvitacion(model.Tipo, emisor, destinatario, comunidad, equipo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Invitacion/Details/5
        public IActionResult Details(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var i = _invitacionCEN.ReadOID_Invitacion(id);
            if (i == null) return NotFound();
            if ((i.Emisor == null || i.Emisor.IdUsuario != userId.Value) && (i.Destinatario == null || i.Destinatario.IdUsuario != userId.Value)) return StatusCode(403);

            var vm = InvitacionAssembler.ConvertENToViewModel(i);
            return View(vm);
        }

        // GET: /Invitacion/Edit/5
        public IActionResult Edit(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var i = _invitacionCEN.ReadOID_Invitacion(id);
            if (i == null) return NotFound();
            if (i.Emisor == null || i.Emisor.IdUsuario != userId.Value) return StatusCode(403);

            var vm = InvitacionAssembler.ConvertENToViewModel(i);
            return View(vm);
        }

        // POST: /Invitacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(InvitacionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var i = _invitacionCEN.ReadOID_Invitacion(model.Id);
            if (i == null) return NotFound();
            if (i.Emisor == null || i.Emisor.IdUsuario != userId.Value) return StatusCode(403);

            i.Estado = model.Estado;
            i.FechaRespuesta = model.FechaRespuesta;
            _invitacionCEN.ModifyInvitacion(i);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Invitacion/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var i = _invitacionCEN.ReadOID_Invitacion(id);
            if (i == null) return NotFound();
            if (i.Emisor == null || i.Emisor.IdUsuario != userId.Value) return StatusCode(403);

            _invitacionCEN.DestroyInvitacion(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

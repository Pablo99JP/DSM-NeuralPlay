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
    public class ReaccionController : BasicController
    {
        private readonly ReaccionCEN _reaccionCEN;
        private readonly PublicacionCEN _publicacionCEN;
        private readonly ComentarioCEN _comentarioCEN;

        public ReaccionController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository, ReaccionCEN reaccionCEN, PublicacionCEN publicacionCEN, ComentarioCEN comentarioCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _reaccionCEN = reaccionCEN;
            _publicacionCEN = publicacionCEN;
            _comentarioCEN = comentarioCEN;
        }

        // GET: /Reaccion
        public IActionResult Index()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var all = _reaccionCEN.ReadAll_Reaccion();
            var vm = ReaccionAssembler.ConvertListENToViewModel(all);
            return View(vm);
        }

        // GET: /Reaccion/Details/5
        public IActionResult Details(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var r = _reaccionCEN.ReadOID_Reaccion(id);
            if (r == null) return NotFound();

            var vm = ReaccionAssembler.ConvertENToViewModel(r);
            return View(vm);
        }

        // GET: /Reaccion/Create?publicacionId=5 or ?comentarioId=3
        public IActionResult Create(long? publicacionId, long? comentarioId)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var vm = new ReaccionViewModel();
            if (publicacionId.HasValue) vm.PublicacionId = (int)publicacionId.Value;
            if (comentarioId.HasValue) vm.ComentarioId = (int)comentarioId.Value;
            return View(vm);
        }

        // POST: /Reaccion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReaccionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var usuario = _usuarioCEN.ReadOID_Usuario(userId.Value);
            if (usuario == null) return NotFound();

            Publicacion? pub = null;
            Comentario? com = null;
            if (model.PublicacionId != 0) pub = _publicacionCEN.ReadOID_Publicacion(model.PublicacionId);
            if (model.ComentarioId != 0) com = _comentarioCEN.ReadOID_Comentario(model.ComentarioId);

            _reaccionCEN.NewReaccion(model.Tipo, usuario, pub, com);

            if (pub != null) return RedirectToAction("Details", "Publicacion", new { id = pub.IdPublicacion });
            if (com != null) return RedirectToAction("Details", "Comentario", new { id = com.IdComentario });

            return RedirectToAction(nameof(Index));
        }

        // GET: /Reaccion/Edit/5
        public IActionResult Edit(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var r = _reaccionCEN.ReadOID_Reaccion(id);
            if (r == null) return NotFound();
            if (r.Autor == null || r.Autor.IdUsuario != userId.Value) return StatusCode(403);

            var vm = ReaccionAssembler.ConvertENToViewModel(r);
            return View(vm);
        }

        // POST: /Reaccion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ReaccionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var r = _reaccionCEN.ReadOID_Reaccion(model.Id);
            if (r == null) return NotFound();
            if (r.Autor == null || r.Autor.IdUsuario != userId.Value) return StatusCode(403);

            r.Tipo = model.Tipo;
            _reaccionCEN.ModifyReaccion(r);

            if (r.Publicacion != null) return RedirectToAction("Details", "Publicacion", new { id = r.Publicacion.IdPublicacion });
            if (r.Comentario != null) return RedirectToAction("Details", "Comentario", new { id = r.Comentario.IdComentario });

            return RedirectToAction(nameof(Index));
        }

        // POST: /Reaccion/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var r = _reaccionCEN.ReadOID_Reaccion(id);
            if (r == null) return NotFound();
            if (r.Autor == null || r.Autor.IdUsuario != userId.Value) return StatusCode(403);

            var pubId = r.Publicacion?.IdPublicacion ?? 0;
            var comId = r.Comentario?.IdComentario ?? 0;
            _reaccionCEN.DestroyReaccion(id);

            if (pubId != 0) return RedirectToAction("Details", "Publicacion", new { id = pubId });
            if (comId != 0) return RedirectToAction("Details", "Comentario", new { id = comId });

            return RedirectToAction(nameof(Index));
        }
    }
}

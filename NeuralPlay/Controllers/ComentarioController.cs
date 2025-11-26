using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Controllers
{
    public class ComentarioController : BasicController
    {
        private readonly ComentarioCEN _comentarioCEN;
        private readonly PublicacionCEN _publicacionCEN;

        public ComentarioController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository, ComentarioCEN comentarioCEN, PublicacionCEN publicacionCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _comentarioCEN = comentarioCEN;
            _publicacionCEN = publicacionCEN;
        }

        // GET: /Comentario
        public IActionResult Index()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var all = _comentarioCEN.ReadAll_Comentario();
            var vm = ComentarioAssembler.ConvertListENToViewModel(all);
            return View(vm);
        }

        // GET: /Comentario/Details/5
        public IActionResult Details(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var c = _comentarioCEN.ReadOID_Comentario(id);
            if (c == null) return NotFound();

            var vm = ComentarioAssembler.ConvertENToViewModel(c);
            return View(vm);
        }

        // GET: /Comentario/Create?publicacionId=5
        public IActionResult Create(long? publicacionId)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!publicacionId.HasValue) return BadRequest();
            var p = _publicacionCEN.ReadOID_Publicacion(publicacionId.Value);
            if (p == null) return NotFound();

            var vm = new ComentarioViewModel { PublicacionId = (int)publicacionId.Value };
            return View(vm);
        }

        // POST: /Comentario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ComentarioViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var usuario = _usuarioCEN.ReadOID_Usuario(userId.Value);
            if (usuario == null) return NotFound();

            var publicacion = _publicacionCEN.ReadOID_Publicacion(model.PublicacionId);
            if (publicacion == null) return NotFound();

            _comentarioCEN.NewComentario(model.Contenido ?? string.Empty, usuario, publicacion);
            return RedirectToAction("Details", "Publicacion", new { id = model.PublicacionId });
        }

        // GET: /Comentario/Edit/5
        public IActionResult Edit(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var c = _comentarioCEN.ReadOID_Comentario(id);
            if (c == null) return NotFound();
            if (c.Autor == null || c.Autor.IdUsuario != userId.Value) return StatusCode(403);

            var vm = ComentarioAssembler.ConvertENToViewModel(c);
            return View(vm);
        }

        // POST: /Comentario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ComentarioViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var c = _comentarioCEN.ReadOID_Comentario(model.Id);
            if (c == null) return NotFound();
            if (c.Autor == null || c.Autor.IdUsuario != userId.Value) return StatusCode(403);

            c.Contenido = model.Contenido ?? string.Empty;
            c.FechaEdicion = System.DateTime.UtcNow;
            _comentarioCEN.ModifyComentario(c);

            return RedirectToAction("Details", "Publicacion", new { id = c.Publicacion?.IdPublicacion });
        }

        // POST: /Comentario/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var c = _comentarioCEN.ReadOID_Comentario(id);
            if (c == null) return NotFound();
            if (c.Autor == null || c.Autor.IdUsuario != userId.Value) return StatusCode(403);

            var pubId = c.Publicacion?.IdPublicacion ?? 0;
            _comentarioCEN.DestroyComentario(id);
            return RedirectToAction("Details", "Publicacion", new { id = pubId });
        }
    }
}

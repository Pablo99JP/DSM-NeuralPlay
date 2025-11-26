using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Controllers
{
    public class PublicacionController : BasicController
    {
        private readonly PublicacionCEN _publicacionCEN;

        public PublicacionController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository, PublicacionCEN publicacionCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _publicacionCEN = publicacionCEN;
        }

        // GET: /Publicacion
        public IActionResult Index()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var all = _publicacionCEN.ReadAll_Publicacion();
            var vm = PublicacionAssembler.ConvertListENToViewModel(all);
            return View(vm);
        }

        // GET: /Publicacion/Details/5
        public IActionResult Details(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var p = _publicacionCEN.ReadOID_Publicacion(id);
            if (p == null) return NotFound();

            var vm = PublicacionAssembler.ConvertENToViewModel(p);
            return View(vm);
        }

        // GET: /Publicacion/Create
        public IActionResult Create()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            return View();
        }

        // POST: /Publicacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PublicacionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var usuario = _usuarioCEN.ReadOID_Usuario(userId.Value);
            if (usuario == null) return NotFound();

            _publicacionCEN.NewPublicacion(model.Contenido ?? string.Empty, null, usuario);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Publicacion/Edit/5
        public IActionResult Edit(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var p = _publicacionCEN.ReadOID_Publicacion(id);
            if (p == null) return NotFound();
            if (p.Autor == null || p.Autor.IdUsuario != userId.Value) return StatusCode(403);

            var vm = PublicacionAssembler.ConvertENToViewModel(p);
            return View(vm);
        }

        // POST: /Publicacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PublicacionViewModel model)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return View(model);

            var p = _publicacionCEN.ReadOID_Publicacion(model.Id);
            if (p == null) return NotFound();
            if (p.Autor == null || p.Autor.IdUsuario != userId.Value) return StatusCode(403);

            p.Contenido = model.Contenido ?? string.Empty;
            p.FechaEdicion = System.DateTime.UtcNow;
            _publicacionCEN.ModifyPublicacion(p);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Publicacion/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var p = _publicacionCEN.ReadOID_Publicacion(id);
            if (p == null) return NotFound();
            if (p.Autor == null || p.Autor.IdUsuario != userId.Value) return StatusCode(403);

            _publicacionCEN.DestroyPublicacion(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

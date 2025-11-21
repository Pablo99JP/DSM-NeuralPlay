using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Controllers
{
    public class UsuarioController : BasicController
    {
        public UsuarioController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository)
            : base(usuarioCEN, usuarioRepository)
        {
        }

        // GET: /Usuario
        public IActionResult Index()
        {
            var usuarios = _usuarioCEN.ReadAll_Usuario();
            var vmList = UsuarioAssembler.ConvertListENToModel(usuarios);
            return View(vmList);
        }

        // GET: /Usuario/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UsuarioViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Simulamos creación: la CEN acepta (nick, correo, hash)
            _usuarioCEN.NewUsuario(model.Nombre ?? string.Empty, model.Email ?? string.Empty, model.Password ?? string.Empty);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/Edit/5
        public IActionResult Edit(long id)
        {
            var en = _usuarioCEN.ReadOID_Usuario(id);
            if (en == null) return NotFound();

            var vm = UsuarioAssembler.ConvertENToViewModel(en);
            return View(vm);
        }

        // POST: /Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UsuarioViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var en = _usuarioCEN.ReadOID_Usuario(model.Id);
            if (en == null) return NotFound();

            // Actualizamos campos relevantes y delegamos a la CEN
            en.Nick = model.Nombre;
            en.CorreoElectronico = model.Email;
            en.ContrasenaHash = model.Password; // En un caso real, hashearíamos

            _usuarioCEN.ModifyUsuario(en);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Usuario/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            _usuarioCEN.DestroyUsuario(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/Details/5
        public IActionResult Details(long id)
        {
            var en = _usuarioCEN.ReadOID_Usuario(id);
            if (en == null) return NotFound();

            var vm = UsuarioAssembler.ConvertENToViewModel(en);
            return View(vm);
        }
    }
}

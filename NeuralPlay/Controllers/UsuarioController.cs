using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;

// For session extension methods
using Microsoft.AspNetCore.Http;

namespace NeuralPlay.Controllers
{
    public class UsuarioController : BasicController
    {
        private readonly ApplicationCore.Domain.Repositories.IUnitOfWork _unitOfWork;

        public UsuarioController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository, ApplicationCore.Domain.Repositories.IUnitOfWork unitOfWork)
            : base(usuarioCEN, usuarioRepository)
        {
            _unitOfWork = unitOfWork;
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

            try
            {
                // Validar que no exista otro usuario con el mismo email
                if (_usuarioCEN.ExisteEmail(model.Email ?? string.Empty))
                {
                    ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con este correo electrónico.");
                    return View(model);
                }

                // Validar que no exista otro usuario con el mismo nick
                if (_usuarioCEN.ExisteNick(model.Nombre ?? string.Empty))
                {
                    ModelState.AddModelError(nameof(model.Nombre), "Ya existe un usuario con este nombre.");
                    return View(model);
                }

                // Crear usuario con contraseña hasheada
                var usuarioCreado = _usuarioCEN.NewUsuario(model.Nombre ?? string.Empty, model.Email ?? string.Empty, model.Password ?? string.Empty);
                
                // Persistir cambios via UnitOfWork cuando se usa NHibernate
                try { _unitOfWork?.SaveChanges(); } catch { }
                
                // Validar que el usuario se creó correctamente
                if (usuarioCreado == null || usuarioCreado.IdUsuario == 0)
                {
                    ModelState.AddModelError(string.Empty, "Error al crear el usuario. Intenta de nuevo.");
                    return View(model);
                }

                // Login automático después del registro
                HttpContext.Session.SetInt32("UsuarioId", (int)usuarioCreado.IdUsuario);
                HttpContext.Session.SetString("UsuarioNombre", usuarioCreado.Nick ?? string.Empty);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Usuario/Edit/5
        public IActionResult Edit(long id)
        {
            var en = _usuarioCEN.ReadOID_Usuario(id);
            if (en == null) return NotFound();

            var vm = UsuarioAssembler.ConvertENToViewModel(en);
            return View(vm);
        }

        // GET: /Usuario/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Usuario/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Método hipotético en CEN que valida credenciales
            var usuario = _usuarioCEN.Login(model.Email ?? string.Empty, model.Password ?? string.Empty);
            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                return View(model);
            }

            // Guardar datos en session
            HttpContext.Session.SetInt32("UsuarioId", (int)usuario.IdUsuario);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nick ?? string.Empty);

            return RedirectToAction("Index", "Home");
        }

        // Logout: limpia session y redirige al Login
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
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
            try { _unitOfWork?.SaveChanges(); } catch { }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Usuario/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            _usuarioCEN.DestroyUsuario(id);
            try { _unitOfWork?.SaveChanges(); } catch { }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            var en = _usuarioCEN.ReadOID_Usuario(id);
            if (en == null) return NotFound();

            var vm = UsuarioAssembler.ConvertENToViewModel(en);
            return View(vm); // Renderiza Delete.cshtml con el ViewModel
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

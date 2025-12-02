using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NeuralPlay.Models;
using NeuralPlay.Models.Assemblers;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Domain.CP;
using NeuralPlay.Services; // Añadir el using para IUsuarioAuth

namespace NeuralPlay.Controllers
{
    public class PerfilesController : BasicController
    {
        private readonly ActualizarPerfilCP _actualizarPerfilCP;
        private readonly IUsuarioAuth _usuarioAuth; // Añadir el servicio de autenticación

        public PerfilesController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            ActualizarPerfilCP actualizarPerfilCP,
            IUsuarioAuth usuarioAuth) // Inyectar el servicio
            : base(usuarioCEN, usuarioRepository)
        {
            _actualizarPerfilCP = actualizarPerfilCP;
            _usuarioAuth = usuarioAuth; // Asignar el servicio
        }

        // GET: Perfiles
        public async Task<IActionResult> Index()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                // --- INICIO DE LA CORRECCIÓN ---
                // Usamos Fetch para cargar el Usuario junto con cada Perfil
                var perfiles = await session.Query<Perfil>()
                                            .Fetch(p => p.Usuario) // Carga ansiosa del usuario
                                            .ToListAsync();
                // --- FIN DE LA CORRECCIÓN ---

                var viewModels = perfiles.Select(p => new PerfilViewModel
                {
                    IdPerfil = p.IdPerfil,
                    IdUsuario = p.Usuario.IdUsuario,
                    NickUsuario = p.Usuario.Nick,
                    Descripcion = p.Descripcion,
                    Avatar = p.FotoPerfilUrl
                }).ToList();

                return View(viewModels);
            }
        }

        // GET: Perfiles/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var session = NHibernateHelper.OpenSession())
            {
                var perfilRepository = new NHibernatePerfilRepository(session);
                var perfilCEN = new PerfilCEN(perfilRepository);
                var perfil = await Task.Run(() => perfilCEN.ReadOID_Perfil(id.Value));

                if (perfil == null)
                {
                    return NotFound();
                }

                var viewModel = new PerfilViewModel
                {
                    IdPerfil = perfil.IdPerfil,
                    IdUsuario = perfil.Usuario.IdUsuario,
                    NickUsuario = perfil.Usuario.Nick,
                    Descripcion = perfil.Descripcion,
                    Avatar = perfil.FotoPerfilUrl,
                    Juegos = perfil.PerfilJuegos?.Select(pj => JuegoAssembler.ToViewModel(pj.Juego)).ToList() ?? new List<JuegoViewModel>()
                };

                return View(viewModel);
            }
        }

        // GET: Perfiles/Feed/5
        public async Task<IActionResult> Feed(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var session = NHibernateHelper.OpenSession())
            {
                var perfilRepository = new NHibernatePerfilRepository(session);
                var perfilCEN = new PerfilCEN(perfilRepository);
                var perfil = await Task.Run(() => perfilCEN.ReadOID_Perfil(id.Value));

                if (perfil == null)
                {
                    return NotFound();
                }

                var viewModel = new PerfilViewModel
                {
                    IdPerfil = perfil.IdPerfil,
                    IdUsuario = perfil.Usuario.IdUsuario,
                    NickUsuario = perfil.Usuario.Nick,
                    Descripcion = perfil.Descripcion,
                    Avatar = perfil.FotoPerfilUrl
                };

                return View(viewModel);
            }
        }

        // GET: Perfiles/AnadirJuego/5
        public async Task<IActionResult> AnadirJuego(long idPerfil)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var juegoRepository = new NHibernateJuegoRepository(session);
                var juegoCEN = new JuegoCEN(juegoRepository);
                var juegos = await Task.Run(() => juegoCEN.ReadAll_Juego());

                var viewModel = new AnadirJuegoViewModel
                {
                    IdPerfil = idPerfil,
                    ListaDeJuegos = new SelectList(juegos, "IdJuego", "NombreJuego")
                };

                return View(viewModel);
            }
        }

        // POST: Perfiles/AnadirJuego
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnadirJuego(AnadirJuegoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (var session = NHibernateHelper.OpenSession())
                {
                    var perfilRepository = new NHibernatePerfilRepository(session);
                    var perfilCEN = new PerfilCEN(perfilRepository);
                    var perfil = await Task.Run(() => perfilCEN.ReadOID_Perfil(viewModel.IdPerfil));

                    var juegoRepository = new NHibernateJuegoRepository(session);
                    var juegoCEN = new JuegoCEN(juegoRepository);
                    var juego = await Task.Run(() => juegoCEN.ReadOID_Juego(viewModel.IdJuegoSeleccionado));

                    if (perfil != null && juego != null)    
                    {
                        var perfilJuegoRepository = new NHibernatePerfilJuegoRepository(session);
                        var perfilJuegoCEN = new PerfilJuegoCEN(perfilJuegoRepository);
                        await Task.Run(() => perfilJuegoCEN.NewPerfilJuego(perfil, juego));
                    }

                    var uow = new NHibernateUnitOfWork(session);
                    uow.SaveChanges();

                    return RedirectToAction(nameof(Details), new { id = viewModel.IdPerfil });
                }
            }

            using (var session = NHibernateHelper.OpenSession())
            {
                var juegoRepository = new NHibernateJuegoRepository(session);
                var juegoCEN = new JuegoCEN(juegoRepository);
                var juegos = await Task.Run(() => juegoCEN.ReadAll_Juego());
                viewModel.ListaDeJuegos = new SelectList(juegos, "IdJuego", "NombreJuego");
            }

            return View(viewModel);
        }

        // GET: Perfiles/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            // --- INICIO DE LA CORRECCIÓN ---
            // Usar el servicio de autenticación basado en sesión
            var currentUser = _usuarioAuth.GetUsuarioActual();
            if (currentUser == null)
            {
                return Unauthorized("Debes iniciar sesión para editar un perfil.");
            }
            // --- FIN DE LA CORRECCIÓN ---

            using (var session = NHibernateHelper.OpenSession())
            {
                var perfil = await session.GetAsync<Perfil>(id.Value);
                if (perfil == null) return NotFound();

                // --- INICIO DE LA CORRECCIÓN ---
                // Comparar IDs directamente
                if (perfil.Usuario.IdUsuario != currentUser.IdUsuario)
                {
                    return Forbid("No tienes permiso para editar este perfil.");
                }
                // --- FIN DE LA CORRECCIÓN ---

                var viewModel = new PerfilEditViewModel
                {
                    IdPerfil = perfil.IdPerfil,
                    NickUsuario = perfil.Usuario.Nick,
                    Descripcion = perfil.Descripcion,
                    FotoPerfilUrl = perfil.FotoPerfilUrl
                };
                return View(viewModel);
            }
        }

        // POST: Perfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, PerfilEditViewModel viewModel)
        {
            if (id != viewModel.IdPerfil) return NotFound();

            // --- INICIO DE LA CORRECCIÓN ---
            // Reutilizar la misma lógica de autorización del GET
            var currentUser = _usuarioAuth.GetUsuarioActual();
            if (currentUser == null)
            {
                return Unauthorized("Debes iniciar sesión para editar un perfil.");
            }

            using (var session = NHibernateHelper.OpenSession())
            {
                var perfil = await session.GetAsync<Perfil>(id);
                if (perfil == null || perfil.Usuario.IdUsuario != currentUser.IdUsuario)
                {
                    return Forbid("No tienes permiso para editar este perfil.");
                }
            }
            // --- FIN DE LA CORRECCIÓN ---

            if (ModelState.IsValid)
            {
                try
                {
                    _actualizarPerfilCP.Ejecutar(
                        viewModel.IdPerfil,
                        viewModel.NickUsuario,
                        viewModel.Descripcion,
                        viewModel.FotoPerfilUrl
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar el perfil: {ex.Message}");
                    return View(viewModel);
                }
                return RedirectToAction(nameof(Details), new { id = viewModel.IdPerfil });
            }
            return View(viewModel);
        }
    }
}
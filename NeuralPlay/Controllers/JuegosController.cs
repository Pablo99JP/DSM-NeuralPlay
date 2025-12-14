using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Models;
using NeuralPlay.Models.Assemblers; // A�adido el using para el Assembler
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeuralPlay.Controllers
{
    public class JuegosController : BasicController
    {
        // El constructor ahora coincide con el de BasicController
        public JuegosController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository) 
            : base(usuarioCEN, usuarioRepository)
        {
        }

        // GET: Juegos
        public async Task<IActionResult> Index()
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession())
            {
                var juegoRepository = new NHibernateJuegoRepository(session);
                var juegoCEN = new JuegoCEN(juegoRepository);
                IList<Juego> listaJuegos = (IList<Juego>)await Task.Run(() => juegoCEN.ReadAll_Juego());

                // Usando el Assembler para el mapeo
                var viewModels = JuegoAssembler.ToViewModel(listaJuegos);

                return View(viewModels);
            }
        }

        // GET: Juegos/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (NHibernate.ISession session = NHibernateHelper.OpenSession())
            {
                var juegoRepository = new NHibernateJuegoRepository(session);
                var juegoCEN = new JuegoCEN(juegoRepository);
                Juego? juego = await Task.Run(() => juegoCEN.ReadOID_Juego(id.Value));

                if (juego == null)
                {
                    return NotFound();
                }

                // Usando el Assembler para el mapeo
                var viewModel = JuegoAssembler.ToViewModel(juego);

                // Verificar si el usuario está loggeado y si ya tiene el juego en su perfil
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                ViewBag.UsuarioLoggeado = usuarioId.HasValue;
                ViewBag.IdPerfil = (long?)null;
                ViewBag.TieneJuego = false;

                if (usuarioId.HasValue)
                {
                    // Obtener el perfil del usuario
                    var usuarioRepository = new NHibernateUsuarioRepository(session);
                    var usuarioCEN = new UsuarioCEN(usuarioRepository);
                    var usuario = await Task.Run(() => usuarioCEN.ReadOID_Usuario(usuarioId.Value));

                    if (usuario?.Perfil != null)
                    {
                        ViewBag.IdPerfil = usuario.Perfil.IdPerfil;

                        var perfilRepository = new NHibernatePerfilRepository(session);
                        var perfilCEN = new PerfilCEN(perfilRepository);
                        var perfil = await Task.Run(() => perfilCEN.ReadOID_Perfil(usuario.Perfil.IdPerfil));

                        if (perfil?.PerfilJuegos != null)
                        {
                            ViewBag.TieneJuego = perfil.PerfilJuegos.Any(pj => pj.Juego.IdJuego == id.Value);
                        }
                    }
                }

                return View(viewModel);
            }
        }

        // GET: Juegos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Juegos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JuegoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (NHibernate.ISession session = NHibernateHelper.OpenSession())
                {
                    var juegoRepository = new NHibernateJuegoRepository(session);
                    var juegoCEN = new JuegoCEN(juegoRepository);
                    
                    await Task.Run(() => juegoCEN.NewJuego(viewModel.NombreJuego!, viewModel.Genero));

                    var uow = new NHibernateUnitOfWork(session);
                    uow.SaveChanges();

                    return RedirectToAction(nameof(Index));
                }
            }
            return View(viewModel);
        }

        // GET: Juegos/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (NHibernate.ISession session = NHibernateHelper.OpenSession())
            {
                var juegoRepository = new NHibernateJuegoRepository(session);
                var juegoCEN = new JuegoCEN(juegoRepository);
                Juego? juego = await Task.Run(() => juegoCEN.ReadOID_Juego(id.Value));

                if (juego == null)
                {
                    return NotFound();
                }

                // Usando el Assembler para el mapeo
                var viewModel = JuegoAssembler.ToViewModel(juego);

                return View(viewModel);
            }
        }

        // POST: Juegos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, JuegoViewModel viewModel)
        {
            if (id != viewModel.IdJuego)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (NHibernate.ISession session = NHibernateHelper.OpenSession())
                {
                    var juegoRepository = new NHibernateJuegoRepository(session);
                    var juegoCEN = new JuegoCEN(juegoRepository);
                    try
                    {
                        Juego? juego = await Task.Run(() => juegoCEN.ReadOID_Juego(viewModel.IdJuego));
                        if (juego != null)
                        {
                            juego.NombreJuego = viewModel.NombreJuego!;
                            juego.Genero = viewModel.Genero;
                            juegoCEN.ModifyJuego(juego);

                            var uow = new NHibernateUnitOfWork(session);
                            uow.SaveChanges();
                        }   
                        else
                        {
                            return NotFound();
                        }
                    }
                    catch (Exception)
                    {
                        if (await Task.Run(() => juegoCEN.ReadOID_Juego(viewModel.IdJuego)) == null)
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(viewModel);
        }

        // GET: Juegos/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (NHibernate.ISession session = NHibernateHelper.OpenSession())
            {
                var juegoRepository = new NHibernateJuegoRepository(session);
                var juegoCEN = new JuegoCEN(juegoRepository);
                Juego? juego = await Task.Run(() => juegoCEN.ReadOID_Juego(id.Value));

                if (juego == null)
                {
                    return NotFound();
                }

                // Usando el Assembler para el mapeo
                var viewModel = JuegoAssembler.ToViewModel(juego);

                return View(viewModel);
            }
        }

        // POST: Juegos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession())
            {
                var juegoRepository = new NHibernateJuegoRepository(session);
                var juegoCEN = new JuegoCEN(juegoRepository);
                
                await Task.Run(() => juegoCEN.DestroyJuego(id));

                var uow = new NHibernateUnitOfWork(session);
                uow.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using System.Linq;

namespace NeuralPlay.Controllers
{
    public class TorneoController : Controller
    {
        private readonly ApplicationCore.Domain.Repositories.IRepository<Torneo> _torneoRepo;
        private readonly PropuestaTorneoCEN _propuestaCEN;
        private readonly NeuralPlay.Services.IUsuarioAuth _usuarioAuth;
        private readonly ApplicationCore.Domain.CEN.MiembroEquipoCEN _miembroEquipoCEN;
        private readonly ApplicationCore.Domain.CEN.MiembroComunidadCEN _miembroComunidadCEN;
        private readonly ApplicationCore.Domain.CEN.EquipoCEN _equipoCEN;
        private readonly ApplicationCore.Domain.CEN.ParticipacionTorneoCEN _participacionTorneoCEN;
        private readonly ApplicationCore.Domain.CEN.TorneoCEN _torneoCEN;

        public TorneoController(ApplicationCore.Domain.Repositories.IRepository<Torneo> torneoRepo,
            PropuestaTorneoCEN propuestaCEN,
            NeuralPlay.Services.IUsuarioAuth usuarioAuth,
            ApplicationCore.Domain.CEN.MiembroEquipoCEN miembroEquipoCEN,
            ApplicationCore.Domain.CEN.MiembroComunidadCEN miembroComunidadCEN,
            ApplicationCore.Domain.CEN.EquipoCEN equipoCEN,
            ApplicationCore.Domain.CEN.ParticipacionTorneoCEN participacionTorneoCEN,
            ApplicationCore.Domain.CEN.TorneoCEN torneoCEN)
        {
            _torneoRepo = torneoRepo;
            _propuestaCEN = propuestaCEN;
            _usuarioAuth = usuarioAuth;
            _miembroEquipoCEN = miembroEquipoCEN;
            _miembroComunidadCEN = miembroComunidadCEN;
            _equipoCEN = equipoCEN;
            _participacionTorneoCEN = participacionTorneoCEN;
            _torneoCEN = torneoCEN;
        }

        public IActionResult Index()
        {
            var torneos = _torneoRepo.ReadAll().ToList();
            return View(torneos);
        }

        public IActionResult Details(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var vm = new NeuralPlay.Models.TorneoDetailsViewModel { Torneo = torneo };

            // Load propuestas and participaciones explicitly because Torneo mapping does not include collections
            var propuestas = _propuestaCEN.ReadAll_PropuestaTorneo().Where(p => p.Torneo != null && p.Torneo.IdTorneo == id).ToList();
            var participaciones = _participacionTorneoCEN.ReadAll_ParticipacionTorneo().Where(p => p.Torneo != null && p.Torneo.IdTorneo == id).ToList();

            vm.Propuestas = propuestas;
            vm.Participaciones = participaciones;

            return View(vm);
        }

        // Accion simplificada para mostrar el formulario de proponer participación
        [HttpGet]
        public IActionResult ProponerParticipacion(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var vm = new NeuralPlay.Models.ProponerParticipacionViewModel { Torneo = torneo };

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                vm.Message = "Debes iniciar sesión para proponer participación.";
                return View(vm);
            }

            // Obtener los miembros de equipo para el usuario (buscar por nick)
            var miembros = _miembroEquipoCEN.BuscarMiembrosEquipoPorNickUsuario(usuario.Nick);
            var equipos = new List<Equipo>();
            foreach (var m in miembros)
            {
                if (m.Equipo != null) equipos.Add(m.Equipo);
            }

            vm.EquiposDisponibles = equipos;
            // Show any TempData messages from previous POST redirect
            if (TempData.ContainsKey("ErrorMessage")) vm.Message = TempData["ErrorMessage"]?.ToString();
            if (TempData.ContainsKey("SuccessMessage")) vm.Message = TempData["SuccessMessage"]?.ToString();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProponerParticipacion(long id, NeuralPlay.Models.ProponerParticipacionViewModel model)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null) return Forbid();

            // Validar equipo
            var equipo = _equipoCEN.ReadOID_Equipo(model.SelectedEquipoId);
            if (equipo == null) return BadRequest("Equipo no válido");

            // 1) Validar que el torneo esté PENDIENTE o ABIERTO
            if (!string.Equals(torneo.Estado, "PENDIENTE", System.StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(torneo.Estado, "ABIERTO", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "No se pueden crear propuestas en un torneo que no está pendiente o abierto.";
                return RedirectToAction(nameof(ProponerParticipacion), new { id = torneo.IdTorneo });
            }

            // 2) Validar que el usuario pertenezca al equipo (cualquier rol, estado ACTIVO)
            var miembrosUsuario = _miembroEquipoCEN.BuscarMiembrosEquipoPorNickUsuario(usuario.Nick);
            var miembroProp = miembrosUsuario.FirstOrDefault(m => m.Equipo != null && m.Equipo.IdEquipo == equipo.IdEquipo);
            if (miembroProp == null || miembroProp.Estado != ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
            {
                TempData["ErrorMessage"] = "Solo miembros activos del equipo pueden proponer participación.";
                return RedirectToAction(nameof(ProponerParticipacion), new { id = torneo.IdTorneo });
            }

            // 3) Evitar propuestas duplicadas (mismo equipo + mismo torneo)
            var exists = _propuestaCEN.ReadAll_PropuestaTorneo().Any(p => p.Torneo != null && p.EquipoProponente != null && p.Torneo.IdTorneo == torneo.IdTorneo && p.EquipoProponente.IdEquipo == equipo.IdEquipo);
            if (exists)
            {
                TempData["ErrorMessage"] = "Ya existe una propuesta de este equipo para este torneo.";
                return RedirectToAction(nameof(ProponerParticipacion), new { id = torneo.IdTorneo });
            }

            // Crear propuesta vía CEN
            var propuesta = _propuestaCEN.NewPropuestaTorneo(equipo, torneo, usuario);
            TempData["SuccessMessage"] = "Propuesta creada correctamente.";

            // Redirigir a detalles del torneo
            return RedirectToAction("Details", new { id = torneo.IdTorneo });
        }

        // GET: /Torneo/Create
        [HttpGet]
        public IActionResult Create()
        {
            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para crear torneos.";
                return RedirectToAction("Login", "Usuario");
            }

            // Verificar si el usuario es LIDER o MODERADOR de alguna comunidad
            var miembrosComunidad = _miembroComunidadCEN.ReadAll_MiembroComunidad()
                .Where(m => m.Usuario != null && m.Usuario.IdUsuario == usuario.IdUsuario)
                .ToList();
            
            var comunidadesValidas = miembrosComunidad
                .Where(mc => (mc.Rol == ApplicationCore.Domain.Enums.RolComunidad.LIDER || 
                             mc.Rol == ApplicationCore.Domain.Enums.RolComunidad.MODERADOR) &&
                            mc.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
                .Select(mc => mc.Comunidad)
                .Where(c => c != null)
                .ToList();

            if (!comunidadesValidas.Any())
            {
                TempData["ErrorMessage"] = "Solo los líderes y moderadores de comunidad pueden crear torneos.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Comunidades = comunidadesValidas;
            return View(new NeuralPlay.Models.TorneoCreateViewModel());
        }

        // POST: /Torneo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NeuralPlay.Models.TorneoCreateViewModel model)
        {
            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para crear torneos.";
                return RedirectToAction("Login", "Usuario");
            }

            // Verificar si el usuario es LIDER o MODERADOR de alguna comunidad
            var miembrosComunidad = _miembroComunidadCEN.ReadAll_MiembroComunidad()
                .Where(m => m.Usuario != null && m.Usuario.IdUsuario == usuario.IdUsuario)
                .ToList();
            
            var comunidadesValidas = miembrosComunidad
                .Where(mc => (mc.Rol == ApplicationCore.Domain.Enums.RolComunidad.LIDER || 
                             mc.Rol == ApplicationCore.Domain.Enums.RolComunidad.MODERADOR) &&
                            mc.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
                .Select(mc => mc.Comunidad)
                .Where(c => c != null)
                .ToList();

            if (!comunidadesValidas.Any())
            {
                TempData["ErrorMessage"] = "Solo los líderes y moderadores de comunidad pueden crear torneos.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Comunidades = comunidadesValidas;
                return View(model);
            }

            // Validar que la comunidad seleccionada sea válida
            var comunidadSeleccionada = comunidadesValidas.FirstOrDefault(c => c!.IdComunidad == model.ComunidadId);
            if (comunidadSeleccionada == null)
            {
                TempData["ErrorMessage"] = "Debes seleccionar una comunidad válida donde eres líder o moderador.";
                ViewBag.Comunidades = comunidadesValidas;
                return View(model);
            }

            try
            {
                // Crear el torneo asociado a la comunidad
                var torneo = _torneoCEN.NewTorneo(
                    model.Nombre,
                    model.FechaInicio,
                    model.Reglas,
                    comunidadSeleccionada, // ComunidadOrganizadora
                    usuario // Creador del torneo
                );

                TempData["SuccessMessage"] = $"Torneo '{torneo.Nombre}' creado correctamente con estado PENDIENTE. Se abrirá automáticamente cuando tenga al menos 2 participaciones.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al crear el torneo: {ex.Message}";
                return View(model);
            }
        }

        // GET: /Torneo/Edit/5
        [HttpGet]
        public IActionResult Edit(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión.";
                return RedirectToAction("Login", "Usuario");
            }

            // Verificar que el usuario sea el creador
            if (torneo.Creador == null || torneo.Creador.IdUsuario != usuario.IdUsuario)
            {
                TempData["ErrorMessage"] = "Solo el creador del torneo puede editarlo.";
                return RedirectToAction(nameof(Index));
            }

            var model = new NeuralPlay.Models.TorneoCreateViewModel
            {
                Nombre = torneo.Nombre,
                FechaInicio = torneo.FechaInicio,
                Reglas = torneo.Reglas
            };

            ViewBag.IdTorneo = id;
            return View(model);
        }

        // POST: /Torneo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, NeuralPlay.Models.TorneoCreateViewModel model)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión.";
                return RedirectToAction("Login", "Usuario");
            }

            // Verificar que el usuario sea el creador
            if (torneo.Creador == null || torneo.Creador.IdUsuario != usuario.IdUsuario)
            {
                TempData["ErrorMessage"] = "Solo el creador del torneo puede editarlo.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.IdTorneo = id;
                return View(model);
            }

            try
            {
                torneo.Nombre = model.Nombre;
                torneo.FechaInicio = model.FechaInicio;
                torneo.Reglas = model.Reglas;

                _torneoCEN.ModifyTorneo(torneo);
                TempData["SuccessMessage"] = "Torneo actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al actualizar el torneo: {ex.Message}";
                ViewBag.IdTorneo = id;
                return View(model);
            }
        }

        // GET: /Torneo/Delete/5
        [HttpGet]
        public IActionResult Delete(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión.";
                return RedirectToAction("Login", "Usuario");
            }

            // Verificar que el usuario sea el creador
            if (torneo.Creador == null || torneo.Creador.IdUsuario != usuario.IdUsuario)
            {
                TempData["ErrorMessage"] = "Solo el creador del torneo puede eliminarlo.";
                return RedirectToAction(nameof(Index));
            }

            return View(torneo);
        }

        // POST: /Torneo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión.";
                return RedirectToAction("Login", "Usuario");
            }

            // Verificar que el usuario sea el creador
            if (torneo.Creador == null || torneo.Creador.IdUsuario != usuario.IdUsuario)
            {
                TempData["ErrorMessage"] = "Solo el creador del torneo puede eliminarlo.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _torneoCEN.DestroyTorneo(id);
                TempData["SuccessMessage"] = "Torneo eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el torneo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Linq;
using System.Collections.Generic;

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
        private readonly ChatEquipoCEN _chatEquipoCEN;
        private readonly MensajeChatCEN _mensajeChatCEN;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly string[] EstadosPermitidos = { "PENDIENTE", "ABIERTO", "FINALIZADO" };

        // Devuelve los equipos en los que el usuario tiene membresía activa
        private List<Equipo> GetEquiposActivosDelUsuario(long usuarioId)
        {
            var equipos = _miembroEquipoCEN.ReadFilter_EquiposByUsuarioMembership(usuarioId) ?? Enumerable.Empty<Equipo>();
            return equipos.Where(e => e != null).ToList();
        }

        public TorneoController(ApplicationCore.Domain.Repositories.IRepository<Torneo> torneoRepo,
            PropuestaTorneoCEN propuestaCEN,
            NeuralPlay.Services.IUsuarioAuth usuarioAuth,
            ApplicationCore.Domain.CEN.MiembroEquipoCEN miembroEquipoCEN,
            ApplicationCore.Domain.CEN.MiembroComunidadCEN miembroComunidadCEN,
            ApplicationCore.Domain.CEN.EquipoCEN equipoCEN,
            ApplicationCore.Domain.CEN.ParticipacionTorneoCEN participacionTorneoCEN,
            ApplicationCore.Domain.CEN.TorneoCEN torneoCEN,
            ChatEquipoCEN chatEquipoCEN,
            MensajeChatCEN mensajeChatCEN,
            IUnitOfWork unitOfWork)
        {
            _torneoRepo = torneoRepo;
            _propuestaCEN = propuestaCEN;
            _usuarioAuth = usuarioAuth;
            _miembroEquipoCEN = miembroEquipoCEN;
            _miembroComunidadCEN = miembroComunidadCEN;
            _equipoCEN = equipoCEN;
            _participacionTorneoCEN = participacionTorneoCEN;
            _torneoCEN = torneoCEN;
            _chatEquipoCEN = chatEquipoCEN;
            _mensajeChatCEN = mensajeChatCEN;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var torneos = _torneoRepo.ReadAll().ToList();
            return View(torneos);
        }

        public IActionResult ByComunidad(long idComunidad)
        {
            var torneos = _torneoRepo.ReadAll()
                .Where(t => t.ComunidadOrganizadora != null && t.ComunidadOrganizadora.IdComunidad == idComunidad)
                .ToList();
            
            ViewBag.ComunidadId = idComunidad;
            return View("Index", torneos);
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

            // Obtener equipos del usuario actual
            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario != null)
            {
                vm.EquiposUsuario = GetEquiposActivosDelUsuario(usuario.IdUsuario);
            }

            // Crear clasificación con 0 puntos por defecto para todos los equipos participantes
            var clasificacion = new Dictionary<long, int>();
            foreach (var p in participaciones.Where(p => p.Equipo != null))
            {
                clasificacion[p.Equipo!.IdEquipo] = 0; // Puntos por defecto
            }
            vm.Clasificacion = clasificacion;

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

            // Obtener solo los equipos donde el usuario tiene membresía ACTIVA
            var equipos = GetEquiposActivosDelUsuario(usuario.IdUsuario);

            vm.EquiposDisponibles = equipos;
            
            // Notificar si el usuario no está en ningún equipo activo
            if (!equipos.Any())
            {
                vm.Message = "No perteneces a ningún equipo activo. Únete a un equipo para poder proponer participación.";
            }
            
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
            var equiposUsuario = GetEquiposActivosDelUsuario(usuario.IdUsuario);
            var pertenece = equiposUsuario.Any(e => e.IdEquipo == equipo.IdEquipo);
            if (!pertenece)
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

            // Publicar en el chat del equipo
            try
            {
                var chat = equipo.Chat;
                if (chat == null)
                {
                    chat = _chatEquipoCEN.NewChatEquipo(equipo);
                    equipo.Chat = chat;
                    _equipoCEN.ModifyEquipo(equipo);
                }

                var contenido = $"{usuario.Nick} ha propuesto participar en el torneo \"{torneo.Nombre}\".";
                _mensajeChatCEN.NewMensajeChat(contenido, usuario, chat);
                try { _unitOfWork?.SaveChanges(); } catch { }
            }
            catch { /* No bloquear si falla el mensaje de notificación */ }

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
                return RedirectToAction("Index", "Comunidad");
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
                return RedirectToAction("Index", "Comunidad");
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
                    model.Premios,
                    comunidadSeleccionada, // ComunidadOrganizadora
                    usuario // Creador del torneo
                );

                TempData["SuccessMessage"] = $"Torneo '{torneo.Nombre}' creado correctamente con estado PENDIENTE. El creador podrá abrir o finalizar el torneo desde la edición.";
                // Redirigir a la comunidad desde la que se creó el torneo
                return RedirectToAction("Details", "Comunidad", new { id = comunidadSeleccionada.IdComunidad });
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
                var comunidadId = torneo.ComunidadOrganizadora?.IdComunidad;
                if (comunidadId.HasValue)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
                }
                return RedirectToAction("Index", "Comunidad");
            }

            var model = new NeuralPlay.Models.TorneoCreateViewModel
            {
                Nombre = torneo.Nombre,
                FechaInicio = torneo.FechaInicio,
                Reglas = torneo.Reglas,
                Premios = torneo.Premios,
                Estado = torneo.Estado
            };

            ViewBag.IdTorneo = id;
            ViewBag.Estados = EstadosPermitidos;
            ViewBag.ComunidadId = torneo.ComunidadOrganizadora?.IdComunidad;
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
                var comunidadId = torneo.ComunidadOrganizadora?.IdComunidad;
                if (comunidadId.HasValue)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
                }
                return RedirectToAction("Index", "Comunidad");
            }

            if (!EstadosPermitidos.Contains(model.Estado ?? string.Empty, System.StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Estado", "Estado no válido.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.IdTorneo = id;
                ViewBag.Estados = EstadosPermitidos;
                return View(model);
            }

            try
            {
                torneo.Nombre = model.Nombre;
                torneo.FechaInicio = model.FechaInicio;
                torneo.Reglas = model.Reglas;
                torneo.Premios = model.Premios;
                torneo.Estado = (model.Estado ?? torneo.Estado).ToUpperInvariant();

                _torneoCEN.ModifyTorneo(torneo);
                TempData["SuccessMessage"] = "Torneo actualizado correctamente.";
                var comunidadId = torneo.ComunidadOrganizadora?.IdComunidad;
                if (comunidadId.HasValue)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
                }

                return RedirectToAction("Index", "Comunidad");
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al actualizar el torneo: {ex.Message}";
                ViewBag.IdTorneo = id;
                ViewBag.Estados = EstadosPermitidos;
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
                var comunidadId = torneo.ComunidadOrganizadora?.IdComunidad;
                if (comunidadId.HasValue)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
                }
                return RedirectToAction("Index", "Comunidad");
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
                var comunidadId = torneo.ComunidadOrganizadora?.IdComunidad;
                if (comunidadId.HasValue)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
                }
                return RedirectToAction("Index", "Comunidad");
            }

            try
            {
                _torneoCEN.DestroyTorneo(id);
                TempData["SuccessMessage"] = "Torneo eliminado correctamente.";
                if (torneo.ComunidadOrganizadora != null)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = torneo.ComunidadOrganizadora.IdComunidad });
                }
                return RedirectToAction("Index", "Comunidad");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el torneo: {ex.Message}";
                if (torneo.ComunidadOrganizadora != null)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = torneo.ComunidadOrganizadora.IdComunidad });
                }
                return RedirectToAction("Index", "Comunidad");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarPosiciones(long id, Dictionary<long, int?> posiciones)
        {
            var torneo = _torneoRepo.ReadById(id);
            if (torneo == null) return NotFound();

            var usuario = _usuarioAuth.GetUsuarioActual();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión.";
                return RedirectToAction("Login", "Usuario");
            }

            if (torneo.Creador == null || torneo.Creador.IdUsuario != usuario.IdUsuario)
            {
                TempData["ErrorMessage"] = "Solo el creador del torneo puede gestionar la clasificación.";
                var comunidadId = torneo.ComunidadOrganizadora?.IdComunidad;
                if (comunidadId.HasValue)
                {
                    return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
                }
                return RedirectToAction("Index", "Comunidad");
            }

            var participaciones = _participacionTorneoCEN.ReadAll_ParticipacionTorneo()
                .Where(p => p.Torneo != null && p.Torneo.IdTorneo == id)
                .ToList();

            foreach (var p in participaciones)
            {
                if (posiciones != null && posiciones.TryGetValue(p.IdParticipacion, out var pos))
                {
                    p.Posicion = pos;
                }
                else
                {
                    p.Posicion = null;
                }

                _participacionTorneoCEN.ModifyParticipacionTorneo(p);
            }

            TempData["SuccessMessage"] = "Clasificación actualizada correctamente.";
            return RedirectToAction("Details", new { id });
        }
    }
}

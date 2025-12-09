using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;

namespace NeuralPlay.Controllers
{
    public class EquipoController : BasicController
    {
        private readonly EquipoCEN _equipoCEN;
        private readonly IRepository<Equipo> _equipoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ChatEquipoCEN _chatEquipoCEN;      // NUEVO
        private readonly MensajeChatCEN _mensajeChatCEN;    // NUEVO
        private readonly ParticipacionTorneoCEN _participacionTorneoCEN;
        private readonly PropuestaTorneoCEN _propuestaCEN;
        private readonly IMiembroEquipoRepository _miembroEquipoRepository;
        private readonly MiembroEquipoCEN _miembroEquipoCEN;
        private readonly MiembroComunidadCEN _miembroComunidadCEN;
        private readonly InvitacionCEN _invitacionCEN;
        private readonly SolicitudIngresoCEN _solicitudIngresoCEN;
        private readonly IWebHostEnvironment _env;

        public EquipoController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            EquipoCEN equipoCEN,
            IRepository<Equipo> equipoRepository,
            IUnitOfWork unitOfWork,
            ChatEquipoCEN chatEquipoCEN,         // NUEVO
            MensajeChatCEN mensajeChatCEN,       // NUEVO
            ParticipacionTorneoCEN participacionTorneoCEN,
            PropuestaTorneoCEN propuestaCEN,
            IMiembroEquipoRepository miembroEquipoRepository,
            MiembroEquipoCEN miembroEquipoCEN,
            MiembroComunidadCEN miembroComunidadCEN,
            InvitacionCEN invitacionCEN,
            SolicitudIngresoCEN solicitudIngresoCEN,
            IWebHostEnvironment env
        )
            : base(usuarioCEN, usuarioRepository)
        {
            _equipoCEN = equipoCEN;
            _equipoRepository = equipoRepository;
            _unitOfWork = unitOfWork;
            _chatEquipoCEN = chatEquipoCEN;          // NUEVO
            _mensajeChatCEN = mensajeChatCEN;        // NUEVO
            _participacionTorneoCEN = participacionTorneoCEN;
            _propuestaCEN = propuestaCEN;
            _miembroEquipoRepository = miembroEquipoRepository;
            _miembroEquipoCEN = miembroEquipoCEN;
            _miembroComunidadCEN = miembroComunidadCEN;
            _invitacionCEN = invitacionCEN;
            _solicitudIngresoCEN = solicitudIngresoCEN;
            _env = env;
        }

        // GET: /Equipo
        public IActionResult Index()
        {
            try
            {
                var list = _equipoCEN.ReadAll_Equipo();
                var vms = EquipoAssembler.ConvertListENToViewModel(list);
                return View(vms);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(Enumerable.Empty<EquipoViewModel>());
            }
        }

        // GET: /Equipo/Details/5
        public IActionResult Details(long id)
        {
            try
            {
                var en = _equipoCEN.ReadOID_Equipo(id);
                if (en == null) return NotFound();
                var vm = EquipoAssembler.ConvertENToViewModel(en);
                
                // Cargar los miembros del equipo
                var miembros = (en.Miembros ?? Enumerable.Empty<MiembroEquipo>())
                    .Select(m => MiembroEquipoAssembler.ConvertENToViewModel(m))
                    .ToList();
                
                ViewBag.Miembros = (object?)miembros;

                // Cargar los torneos en los que participa el equipo
                var torneos = _participacionTorneoCEN.ReadAll_ParticipacionTorneo()
                    .Where(p => p.Equipo != null && p.Equipo.IdEquipo == id)
                    .Select(p => p.Torneo)
                    .Where(t => t != null)
                    .Distinct()
                    .ToList();
                
                ViewBag.Torneos = (object?)torneos;

                // Cargar el palmarés (torneos finalizados con su posición)
                var palmares = new Dictionary<ApplicationCore.Domain.EN.Torneo, int>();
                var torneosFinalizados = torneos.Where(t => t.Estado == "FINALIZADO").ToList();
                
                foreach (var torneo in torneosFinalizados)
                {
                    // Obtener todas las participaciones del torneo
                    var participacionesTorneo = _participacionTorneoCEN.ReadAll_ParticipacionTorneo()
                        .Where(p => p.Torneo != null && p.Torneo.IdTorneo == torneo.IdTorneo)
                        .ToList();
                    
                    // Calcular la posición del equipo (todos tienen 0 puntos por defecto, así que es el orden de participación)
                    var posicion = participacionesTorneo
                        .OrderBy(p => p.IdParticipacion)
                        .ToList()
                        .FindIndex(p => p.Equipo != null && p.Equipo.IdEquipo == id) + 1;
                    
                    if (posicion > 0)
                    {
                        palmares[torneo] = posicion;
                    }
                }
                
                ViewBag.Palmares = (object?)palmares;

                // Usuario actual para lógica de votos
                long? currentUserId = null;
                if (User?.Identity?.IsAuthenticated == true)
                {
                    var nameId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (long.TryParse(nameId, out var parsed)) currentUserId = parsed;
                }
                if (currentUserId == null && HttpContext?.Session != null)
                {
                    currentUserId = HttpContext.Session.GetInt32("UsuarioId");
                }

                // Propuestas de torneo del equipo (pendientes) para votar
                var propuestas = _propuestaCEN.ReadAll_PropuestaTorneo()
                    .Where(p => p.EquipoProponente != null && p.EquipoProponente.IdEquipo == id)
                    .Where(p => p.Estado == EstadoSolicitud.PENDIENTE)
                    .Select(p => new
                    {
                        Id = p.IdPropuesta,
                        Nombre = p.Torneo?.Nombre ?? "Torneo propuesto",
                        PropuestoPor = p.PropuestoPor?.Nick ?? "Desconocido",
                        VotosAFavor = p.Votos?.Count(v => v.Valor) ?? 0,
                        VotosEnContra = p.Votos?.Count(v => !v.Valor) ?? 0,
                        Pendientes = System.Math.Max(0, miembros.Count - (p.Votos?.Count ?? 0)),
                        HasVotado = currentUserId != null && (p.Votos?.Any(v => v.Votante != null && v.Votante.IdUsuario == currentUserId.Value) ?? false)
                    })
                    .ToList();

                ViewBag.Propuestas = (object?)propuestas;

                // Determinar si el usuario actual es admin del equipo
                bool isLeader = false;
                if (currentUserId.HasValue)
                {
                    var membership = _miembroEquipoRepository.ReadAll()
                        .FirstOrDefault(m =>
                            m.Usuario != null && m.Usuario.IdUsuario == currentUserId.Value &&
                            m.Equipo != null && m.Equipo.IdEquipo == id &&
                            m.Estado == EstadoMembresia.ACTIVA);
                    isLeader = membership?.Rol == RolEquipo.ADMIN;
                }
                ViewBag.IsLeader = isLeader;
                ViewBag.EquipoId = id;
                ViewBag.CurrentUserId = currentUserId;
                
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: /Equipo/Chat/5
        public IActionResult Chat(long id)
        {
            try
            {
                var equipo = _equipoCEN.ReadOID_Equipo(id);
                if (equipo == null)
                {
                    return NotFound();
                }

                // Asegurar que el equipo tenga chat asociado y persistido
                if (equipo.Chat == null)
                {
                    var nuevoChat = _chatEquipoCEN.NewChatEquipo(equipo);
                    equipo.Chat = nuevoChat;
                    _equipoCEN.ModifyEquipo(equipo);
                    try { _unitOfWork?.SaveChanges(); } catch { }
                }

                // Carga expl�cita de mensajes por chatId para evitar problemas de lazy/eager loading
                var mensajesVm = new List<MensajeChatViewModel>();
                if (equipo.Chat != null)
                {
                    var mensajes = _mensajeChatCEN
                        .ReadByChatId(equipo.Chat.IdChatEquipo)
                        .OrderBy(m => m.FechaEnvio)
                        .ToList();

                    mensajesVm = mensajes.Select(m => new MensajeChatViewModel
                    {
                        Contenido = m.Contenido,
                        NickAutor = m.Autor?.Nick ?? "Desconocido",
                        FechaEnvio = m.FechaEnvio,
                        FotoPerfilUrl = m.Autor?.Perfil?.FotoPerfilUrl
                    }).ToList();
                }

                var chatViewModel = new EquipoChatViewModel
                {
                    IdEquipo = equipo.IdEquipo,
                    NombreEquipo = equipo.Nombre,
                    ImagenUrl = equipo.ImagenUrl,
                    Mensajes = mensajesVm
                };

                return View(chatViewModel);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Equipo/EnviarMensaje/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarMensaje(long id, string contenido)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contenido))
                {
                    TempData["Error"] = "El mensaje no puede estar vac�o.";
                    return RedirectToAction(nameof(Chat), new { id });
                }

                var userId = HttpContext?.Session?.GetInt32("UsuarioId");
                if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

                var usuario = _usuarioCEN.ReadOID_Usuario(userId.Value);
                if (usuario == null) return RedirectToAction("Login", "Usuario");

                var equipo = _equipoCEN.ReadOID_Equipo(id);
                if (equipo == null) return NotFound();

                // Asegurar que el equipo tenga chat asociado
                var chat = equipo.Chat;
                if (chat == null)
                {
                    chat = _chatEquipoCEN.NewChatEquipo(equipo);
                    equipo.Chat = chat;
                    _equipoCEN.ModifyEquipo(equipo);
                    try { _unitOfWork?.SaveChanges(); } catch { }
                }

                // Crear y persistir el mensaje
                _mensajeChatCEN.NewMensajeChat(contenido.Trim(), usuario, chat);

                try { _unitOfWork?.SaveChanges(); } catch { }

                return RedirectToAction(nameof(Chat), new { id });
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error al enviar el mensaje: {ex.Message}";
                return RedirectToAction(nameof(Chat), new { id });
            }
        }

        // GET: /Equipo/Create
        public IActionResult Create()
        {
            // El usuario debe estar autenticado y pertenecer a alguna comunidad activa
            var userId = HttpContext.Session.GetInt32("UsuarioId");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para crear un equipo.";
                return RedirectToAction("Login", "Usuario");
            }

            var isInCommunity = _miembroComunidadCEN.ReadAll_MiembroComunidad()
                .Any(m => m.Usuario != null && m.Usuario.IdUsuario == userId.Value &&
                          m.Estado == EstadoMembresia.ACTIVA);

            if (!isInCommunity)
            {
                TempData["ErrorMessage"] = "Debes pertenecer a una comunidad activa para crear un equipo.";
                return RedirectToAction("Index", "Comunidad");
            }

            return View();
        }

        // POST: /Equipo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EquipoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var userId = HttpContext.Session.GetInt32("UsuarioId");
                if (!userId.HasValue)
                {
                    TempData["ErrorMessage"] = "Debes iniciar sesión para crear un equipo.";
                    return RedirectToAction("Login", "Usuario");
                }

                // Validar pertenencia a una comunidad activa
                var isInCommunity = _miembroComunidadCEN.ReadAll_MiembroComunidad()
                    .Any(m => m.Usuario != null && m.Usuario.IdUsuario == userId.Value &&
                              m.Estado == EstadoMembresia.ACTIVA);

                if (!isInCommunity)
                {
                    TempData["ErrorMessage"] = "Debes pertenecer a una comunidad activa para crear un equipo.";
                    return RedirectToAction("Index", "Comunidad");
                }

                var usuario = _usuarioRepository.ReadById(userId.Value);
                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "No se pudo identificar al usuario.";
                    return RedirectToAction("Login", "Usuario");
                }

                string? imagenPath = null;

                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    var ext = Path.GetExtension(model.ImagenArchivo.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var folder = Path.Combine(_env.WebRootPath, "Recursos", "Equipos");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    var fullPath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        model.ImagenArchivo.CopyTo(stream);
                    }
                    imagenPath = $"/Recursos/Equipos/{fileName}";
                }

                var cen = new EquipoCEN(_equipoRepository);
                var equipo = cen.NewEquipo(model.Nombre ?? string.Empty, model.Descripcion, model.Actividad, model.Pais, model.Idioma);

                if (imagenPath != null)
                {
                    equipo.ImagenUrl = imagenPath;
                    _equipoCEN.ModifyEquipo(equipo);
                }

                // Añadir creador como miembro ADMIN del equipo
                _miembroEquipoCEN.NewMiembroEquipo(usuario, equipo, RolEquipo.ADMIN);

                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction("Index", "Equipos");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al crear el equipo: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Equipo/Edit/5
        public IActionResult Edit(long id)
        {
            try
            {
                var en = _equipoCEN.ReadOID_Equipo(id);
                if (en == null) return NotFound();
                var vm = EquipoAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Equipo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EquipoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var en = _equipoCEN.ReadOID_Equipo(model.IdEquipo);
                if (en == null) return NotFound();

                en.Nombre = model.Nombre ?? en.Nombre;
                en.Descripcion = model.Descripcion;
                en.Actividad = model.Actividad;
                en.Pais = model.Pais;
                en.Idioma = model.Idioma;
                // FechaCreacion no se modifica normalmente

                // Guardar nuevo logo si se envía
                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    var ext = Path.GetExtension(model.ImagenArchivo.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var folder = Path.Combine(_env.WebRootPath, "Recursos", "Equipos");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    var fullPath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        model.ImagenArchivo.CopyTo(stream);
                    }
                    en.ImagenUrl = $"/Recursos/Equipos/{fileName}";
                }

                _equipoCEN.ModifyEquipo(en);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction("Index", "Equipos");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: /Equipo/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            try
            {
                var en = _equipoCEN.ReadOID_Equipo(id);
                if (en == null) return NotFound();
                var vm = EquipoAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Equipo/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            try
            {
                var equipo = _equipoCEN.ReadOID_Equipo(id);
                if (equipo == null) return NotFound();

                // Eliminar dependencias que tienen FK al equipo
                var invitaciones = _invitacionCEN.ReadAll_Invitacion()
                    .Where(i => i.Equipo != null && i.Equipo.IdEquipo == id)
                    .ToList();
                foreach (var inv in invitaciones)
                {
                    _invitacionCEN.DestroyInvitacion(inv.IdInvitacion);
                }

                var solicitudes = _solicitudIngresoCEN.ReadAll_SolicitudIngreso()
                    .Where(s => s.Equipo != null && s.Equipo.IdEquipo == id)
                    .ToList();
                foreach (var sol in solicitudes)
                {
                    _solicitudIngresoCEN.DestroySolicitudIngreso(sol.IdSolicitud);
                }

                var participaciones = _participacionTorneoCEN.ReadAll_ParticipacionTorneo()
                    .Where(p => p.Equipo != null && p.Equipo.IdEquipo == id)
                    .ToList();
                foreach (var par in participaciones)
                {
                    _participacionTorneoCEN.DestroyParticipacionTorneo(par.IdParticipacion);
                }

                var propuestas = _propuestaCEN.ReadAll_PropuestaTorneo()
                    .Where(p => p.EquipoProponente != null && p.EquipoProponente.IdEquipo == id)
                    .ToList();
                foreach (var pro in propuestas)
                {
                    _propuestaCEN.DestroyPropuestaTorneo(pro.IdPropuesta);
                }

                var miembros = _miembroEquipoRepository.ReadAll()
                    .Where(m => m.Equipo != null && m.Equipo.IdEquipo == id)
                    .ToList();
                foreach (var mem in miembros)
                {
                    _miembroEquipoCEN.DestroyMiembroEquipo(mem.IdMiembroEquipo);
                }

                // Chat del equipo (los mensajes se borran por cascade all-delete-orphan)
                if (equipo.Chat != null)
                {
                    _chatEquipoCEN.DestroyChatEquipo(equipo.Chat.IdChatEquipo);
                }

                _equipoCEN.DestroyEquipo(id);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction("Index", "Equipos");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(DeleteConfirm), new { id });
            }
        }
    }
}

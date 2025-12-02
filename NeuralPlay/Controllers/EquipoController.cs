using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;
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

        public EquipoController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            EquipoCEN equipoCEN,
            IRepository<Equipo> equipoRepository,
            IUnitOfWork unitOfWork,
            ChatEquipoCEN chatEquipoCEN,         // NUEVO
            MensajeChatCEN mensajeChatCEN        // NUEVO
        )
            : base(usuarioCEN, usuarioRepository)
        {
            _equipoCEN = equipoCEN;
            _equipoRepository = equipoRepository;
            _unitOfWork = unitOfWork;
            _chatEquipoCEN = chatEquipoCEN;          // NUEVO
            _mensajeChatCEN = mensajeChatCEN;        // NUEVO
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
                        FechaEnvio = m.FechaEnvio
                    }).ToList();
                }

                var chatViewModel = new EquipoChatViewModel
                {
                    IdEquipo = equipo.IdEquipo,
                    NombreEquipo = equipo.Nombre,
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
                var cen = new EquipoCEN(_equipoRepository);
                cen.NewEquipo(model.Nombre ?? string.Empty, model.Descripcion, model.Actividad, model.Pais, model.Idioma);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
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

                _equipoCEN.ModifyEquipo(en);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
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
                _equipoCEN.DestroyEquipo(id);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(DeleteConfirm), new { id });
            }
        }
    }
}

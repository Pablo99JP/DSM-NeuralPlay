using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Models;
using NHibernate.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace NeuralPlay.Controllers
{
    public class ChatController : BasicController
    {
        private readonly ChatEquipoCEN _chatEquipoCEN;

        public ChatController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            ChatEquipoCEN chatEquipoCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _chatEquipoCEN = chatEquipoCEN;
        }

        // GET: Chat/VerChat/5
        public async Task<IActionResult> VerChat(long idEquipo)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                // --- INICIO DE LA CORRECCI�N ---
                // La consulta debe partir de Equipo, que es la entidad que tiene la relaci�n con el Chat.
                var equipoConChat = await session.Query<Equipo>()
                                                 .Where(e => e.IdEquipo == idEquipo)
                                                 .Fetch(e => e.Chat) // Carga el chat asociado al equipo
                                                 .ThenFetchMany(c => c.Mensajes) // Del chat, carga sus mensajes
                                                 .ThenFetch(m => m.Autor) // De cada mensaje, carga su autor
                                                 .SingleOrDefaultAsync();

                if (equipoConChat == null || equipoConChat.Chat == null)
                {
                    return NotFound($"No se encontr� un equipo o un chat para el ID {idEquipo}.");
                }

                var chatEquipo = equipoConChat.Chat;
                // --- FIN DE LA CORRECCI�N ---

                // Mapeamos las entidades a nuestro ViewModel
                var viewModel = new ChatEquipoViewModel
                {
                    IdChatEquipo = chatEquipo.IdChatEquipo,
                    NombreEquipo = equipoConChat.Nombre ?? "Sin nombre", // El nombre se obtiene del equipo
                    Mensajes = chatEquipo.Mensajes
                                         .OrderBy(m => m.FechaEnvio)
                                         .Select(m => new MensajeChatViewModel
                                         {
                                             Contenido = m.Contenido,
                                             FechaEnvio = m.FechaEnvio,
                                             NickAutor = m.Autor?.Nick ?? "Desconocido"
                                         }).ToList()
                };

                return View(viewModel);
            }
        }
    }
}
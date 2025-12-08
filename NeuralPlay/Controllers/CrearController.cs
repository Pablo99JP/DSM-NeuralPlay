using Microsoft.AspNetCore.Mvc;

namespace NeuralPlay.Controllers
{
    public class CrearController : Controller
    {
        // GET: /Crear
        public IActionResult Index()
        {
            // Verificar si el usuario está autenticado
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue)
            {
                return RedirectToAction("Login", "Usuario");
            }

            return View();
        }
    }
}

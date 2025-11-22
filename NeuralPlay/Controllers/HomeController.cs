using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Models;
using System.Diagnostics;
using NHibernate;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN; // Aseg�rate de tener esta referencia si usas NHibernate

namespace NeuralPlay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private NHibernate.ISession _session; // Agrega este campo si tienes acceso a ISession


        public IActionResult Index()
        {
            // Deshabilitado: conexión SQL y NHibernate
            // _session = NHibernateHelper.OpenSession();
            // var juegoRepository = new NHibernateJuegoRepository(_session);
            // JuegoCEN juegoCEN = new JuegoCEN(juegoRepository);
            // IList<Juego> listaJuegos = juegoCEN.ReadAll_Juego().ToList();
            // _session.Close();

            // Si la vista espera una lista, pasar una lista vacía
            return View(System.Linq.Enumerable.Empty<object>());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

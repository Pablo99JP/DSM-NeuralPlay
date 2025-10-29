using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    // Example CP (use-case) that orchestrates creating a Comunidad
    public class CrearComunidadCP
    {
        private readonly IRepository<Comunidad> _comunidadRepo;
        private readonly IUnitOfWork _uow;

        public CrearComunidadCP(IRepository<Comunidad> comunidadRepo, IUnitOfWork uow)
        {
            _comunidadRepo = comunidadRepo;
            _uow = uow;
        }

        public Comunidad Ejecutar(string nombre, string? descripcion = null)
        {
            var c = new Comunidad
            {
                Nombre = nombre,
                Descripcion = descripcion,
                FechaCreacion = System.DateTime.UtcNow
            };
            _comunidadRepo.New(c);
            _uow.SaveChanges();
            return c;
        }
    }
}

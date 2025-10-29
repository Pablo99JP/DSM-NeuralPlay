using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ComunidadCEN
    {
        private readonly IRepository<Comunidad> _repo;

        public ComunidadCEN(IRepository<Comunidad> repo)
        {
            _repo = repo;
        }

        public Comunidad NewComunidad(string nombre, string? descripcion = null)
        {
            var c = new Comunidad { Nombre = nombre, Descripcion = descripcion, FechaCreacion = System.DateTime.UtcNow };
            _repo.New(c);
            return c;
        }

        public Comunidad? ReadOID_Comunidad(long id) => _repo.ReadById(id);
        public IEnumerable<Comunidad> ReadAll_Comunidad() => _repo.ReadAll();
        public void ModifyComunidad(Comunidad c) => _repo.Modify(c);
        public void DestroyComunidad(long id) => _repo.Destroy(id);
        public IEnumerable<Comunidad> ReadFilter_Comunidad(string filter) => _repo.ReadFilter(filter);
    }
}

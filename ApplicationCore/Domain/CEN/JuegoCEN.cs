using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class JuegoCEN
    {
        private readonly IRepository<Juego> _repo;

        public JuegoCEN(IRepository<Juego> repo)
        {
            _repo = repo;
        }

        public Juego NewJuego(string nombreJuego, ApplicationCore.Domain.Enums.GeneroJuego genero)
        {
            var j = new Juego { NombreJuego = nombreJuego, Genero = genero };
            _repo.New(j);
            return j;
        }

        public Juego? ReadOID_Juego(long id) => _repo.ReadById(id);
        public IEnumerable<Juego> ReadAll_Juego() => _repo.ReadAll();
        public void ModifyJuego(Juego j) => _repo.Modify(j);
        public void DestroyJuego(long id) => _repo.Destroy(id);
        public IEnumerable<Juego> ReadFilter_Juego(string filter) => _repo.ReadFilter(filter);
    }
}

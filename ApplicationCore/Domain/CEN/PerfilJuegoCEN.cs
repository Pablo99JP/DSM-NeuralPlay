using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class PerfilJuegoCEN
    {
        private readonly IRepository<PerfilJuego> _repo;

        public PerfilJuegoCEN(IRepository<PerfilJuego> repo)
        {
            _repo = repo;
        }

        public PerfilJuego NewPerfilJuego(Perfil perfil, Juego juego)
        {
            var pj = new PerfilJuego { Perfil = perfil, Juego = juego, FechaAdicion = System.DateTime.UtcNow };
            _repo.New(pj);
            return pj;
        }

        public PerfilJuego? ReadOID_PerfilJuego(long id) => _repo.ReadById(id);
        public IEnumerable<PerfilJuego> ReadAll_PerfilJuego() => _repo.ReadAll();
        public void ModifyPerfilJuego(PerfilJuego pj) => _repo.Modify(pj);
        public void DestroyPerfilJuego(long id) => _repo.Destroy(id);
        public IEnumerable<PerfilJuego> BuscarPerfilesPorNombreJuego(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarPerfilesPorNombreJuego");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<PerfilJuego> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}

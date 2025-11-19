using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class SesionCEN
    {
        private readonly IRepository<Sesion> _repo;

        public SesionCEN(IRepository<Sesion> repo)
        {
            _repo = repo;
        }

        public Sesion NewSesion(Usuario usuario, string token)
        {
            var s = new Sesion { Usuario = usuario, Token = token, FechaInicio = System.DateTime.UtcNow };
            _repo.New(s);
            return s;
        }

        public Sesion? ReadOID_Sesion(long id) => _repo.ReadById(id);
        public IEnumerable<Sesion> ReadAll_Sesion() => _repo.ReadAll();
        public void ModifySesion(Sesion s) => _repo.Modify(s);
        public void DestroySesion(long id) => _repo.Destroy(id);
        public IEnumerable<Sesion> BuscarSesionesPorToken(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarSesionesPorToken");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<Sesion> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}

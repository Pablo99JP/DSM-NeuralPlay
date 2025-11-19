using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.CEN
{
    public class ReaccionCEN
    {
        private readonly IRepository<Reaccion> _repo;

        public ReaccionCEN(IRepository<Reaccion> repo)
        {
            _repo = repo;
        }

        public Reaccion NewReaccion(TipoReaccion tipo, Usuario autor, Publicacion? publicacion = null, Comentario? comentario = null)
        {
            var r = new Reaccion { Tipo = tipo, FechaCreacion = System.DateTime.UtcNow, Autor = autor, Publicacion = publicacion, Comentario = comentario };
            _repo.New(r);
            return r;
        }

        public Reaccion? ReadOID_Reaccion(long id) => _repo.ReadById(id);
        public IEnumerable<Reaccion> ReadAll_Reaccion() => _repo.ReadAll();
        public void ModifyReaccion(Reaccion r) => _repo.Modify(r);
        public void DestroyReaccion(long id) => _repo.Destroy(id);
        public IEnumerable<Reaccion> BuscarReaccionesPorAutorNickOPublicacionContenido(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarReaccionesPorAutorNickOPublicacionContenido");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<Reaccion> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}

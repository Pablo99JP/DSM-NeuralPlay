using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ComentarioCEN
    {
        private readonly IRepository<Comentario> _repo;

        public ComentarioCEN(IRepository<Comentario> repo)
        {
            _repo = repo;
        }

        public Comentario NewComentario(string contenido, Usuario? autor = null, Publicacion? publicacion = null)
        {
            var c = new Comentario { Contenido = contenido, FechaCreacion = System.DateTime.UtcNow, Autor = autor, Publicacion = publicacion };
            _repo.New(c);
            return c;
        }

        public Comentario? ReadOID_Comentario(long id) => _repo.ReadById(id);
        public IEnumerable<Comentario> ReadAll_Comentario() => _repo.ReadAll();
        public void ModifyComentario(Comentario c) => _repo.Modify(c);
        public void DestroyComentario(long id) => _repo.Destroy(id);
        public IEnumerable<Comentario> BuscarComentariosPorContenido(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarComentariosPorContenido");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<Comentario> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}

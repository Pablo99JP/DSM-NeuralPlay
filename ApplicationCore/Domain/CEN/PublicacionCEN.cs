using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class PublicacionCEN
    {
        private readonly IRepository<Publicacion> _repo;

        public PublicacionCEN(IRepository<Publicacion> repo)
        {
            _repo = repo;
        }

        public Publicacion NewPublicacion(string contenido, Comunidad? comunidad = null, Usuario? autor = null)
        {
            var p = new Publicacion { Contenido = contenido, FechaCreacion = System.DateTime.UtcNow, Comunidad = comunidad, Autor = autor };
            _repo.New(p);
            return p;
        }

        public Publicacion? ReadOID_Publicacion(long id) => _repo.ReadById(id);
        public IEnumerable<Publicacion> ReadAll_Publicacion() => _repo.ReadAll();
        public void ModifyPublicacion(Publicacion p) => _repo.Modify(p);
        public void DestroyPublicacion(long id) => _repo.Destroy(id);
        public IEnumerable<Publicacion> BuscarPublicacionesPorContenido(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarPublicacionesPorContenido");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<Publicacion> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }

        // Custom: add comentario
        public Comentario AddComentario(Publicacion publicacion, Usuario autor, string contenido)
        {
            var c = new Comentario { Contenido = contenido, FechaCreacion = System.DateTime.UtcNow, Autor = autor, Publicacion = publicacion };
            publicacion.Comentarios.Add(c);
            return c;
        }
    }
}

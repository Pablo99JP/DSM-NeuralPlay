using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateMiembroComunidadRepository : IMiembroComunidadRepository
    {
        private readonly ISession _session;

        public NHibernateMiembroComunidadRepository(ISession session)
        {
            _session = session;
        }

        public MiembroComunidad? ReadById(long id) => _session.Get<MiembroComunidad>(id);

        public IEnumerable<MiembroComunidad> ReadAll()
        {
            var q = _session.CreateQuery("from MiembroComunidad");
            return q.List<MiembroComunidad>();
        }

        public IEnumerable<MiembroComunidad> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from MiembroComunidad m where m.Usuario is not null and lower(m.Usuario.Nick) like :f");
            q.SetParameter("f", f);
            return q.List<MiembroComunidad>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<MiembroComunidad> BuscarMiembrosComunidadPorNickUsuario(string filtro) => ReadFilter(filtro);

        public void New(MiembroComunidad entity) => _session.Save(entity);
        public void Modify(MiembroComunidad entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        public IEnumerable<Usuario> GetUsuariosByComunidad(long idComunidad)
        {
            var q = _session.CreateQuery("select distinct m.Usuario from MiembroComunidad m where m.Comunidad is not null and m.Comunidad.IdComunidad = :idComunidad and m.Usuario is not null");
            q.SetParameter("idComunidad", idComunidad);
            return q.List<Usuario>();
        }
    }
}

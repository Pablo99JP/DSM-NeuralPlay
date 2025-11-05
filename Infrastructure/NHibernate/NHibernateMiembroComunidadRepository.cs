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

        public IEnumerable<MiembroComunidad> ReadAll() => _session.Query<MiembroComunidad>().ToList();

        public IEnumerable<MiembroComunidad> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<MiembroComunidad>().Where(m => m.Usuario != null && m.Usuario.Nick.ToLower().Contains(f)).ToList();
        }

        public void New(MiembroComunidad entity) => _session.Save(entity);
        public void Modify(MiembroComunidad entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        public IEnumerable<Usuario> GetUsuariosByComunidad(long idComunidad)
        {
            return _session.Query<MiembroComunidad>()
                .Where(m => m.Comunidad != null && m.Comunidad.IdComunidad == idComunidad && m.Usuario != null)
                .Select(m => m.Usuario)
                .Distinct()
                .ToList();
        }
    }
}

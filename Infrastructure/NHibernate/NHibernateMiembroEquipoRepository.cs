using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateMiembroEquipoRepository : IRepository<MiembroEquipo>
    {
        private readonly ISession _session;

        public NHibernateMiembroEquipoRepository(ISession session)
        {
            _session = session;
        }

        public MiembroEquipo? ReadById(long id) => _session.Get<MiembroEquipo>(id);

        public IEnumerable<MiembroEquipo> ReadAll() => _session.Query<MiembroEquipo>().ToList();

        public IEnumerable<MiembroEquipo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<MiembroEquipo>().Where(m => m.Usuario != null && m.Usuario.Nick.ToLower().Contains(f)).ToList();
        }

        public void New(MiembroEquipo entity) => _session.Save(entity);
        public void Modify(MiembroEquipo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class GenericRepository<T, TId> : IRepository<T, TId> where T : class
    {
        protected readonly ISession _session;

        public GenericRepository(ISession session)
        {
            _session = session;
        }

        public T DamePorOID(TId id)
        {
            return _session.Get<T>(id);
        }

        public IList<T> DameTodos()
        {
            return _session.Query<T>().ToList();
        }

        public void New(T entity)
        {
            _session.Save(entity);
        }

        public void Modify(T entity)
        {
            _session.Update(entity);
        }

        public void Destroy(TId id)
        {
            var entity = DamePorOID(id);
            if (entity != null)
            {
                _session.Delete(entity);
            }
        }

        public void ModifyAll(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _session.Update(entity);
            }
        }
    }
}

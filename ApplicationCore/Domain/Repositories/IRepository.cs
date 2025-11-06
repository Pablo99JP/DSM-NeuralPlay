using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IRepository<T, TId> where T : class
    {
        T DamePorOID(TId id);
        IList<T> DameTodos();
        void New(T entity);
        void Modify(T entity);
        void Destroy(TId id);
        void ModifyAll(IEnumerable<T> entities);
    }
}

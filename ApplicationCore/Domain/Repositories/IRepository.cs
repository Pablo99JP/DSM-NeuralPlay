using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IRepository<T>
    {
        T? ReadById(long id);
        IEnumerable<T> ReadAll();
        IEnumerable<T> ReadFilter(string filter);
        void New(T entity);
        void Modify(T entity);
        void Destroy(long id);
    }
}

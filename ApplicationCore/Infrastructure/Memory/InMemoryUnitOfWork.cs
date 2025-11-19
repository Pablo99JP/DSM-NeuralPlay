using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Infrastructure.Memory
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        // Synchronous no-op UoW for in-memory testing
        public void SaveChanges() { }
    }
}

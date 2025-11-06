using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.UnitOfWork
{
    public class NHibernateUnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;
        private ITransaction _transaction;

        public NHibernateUnitOfWork(ISession session)
        {
            _session = session;
            _transaction = _session.BeginTransaction();
        }

        public void SaveChanges()
        {
            if (_transaction != null && _transaction.IsActive)
            {
                _transaction.Commit();
                _transaction = _session.BeginTransaction();
            }
        }

        public void Dispose()
        {
            if (_transaction != null && _transaction.IsActive)
            {
                _transaction.Rollback();
            }
            _transaction?.Dispose();
        }
    }
}

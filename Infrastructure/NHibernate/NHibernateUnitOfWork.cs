using NHibernate;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateUnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;
        private ITransaction? _tx;

        public NHibernateUnitOfWork(ISession session)
        {
            _session = session;
            _tx = _session.BeginTransaction();
        }

        public void SaveChanges()
        {
            if (_tx == null) return;
            if (!_tx.IsActive) return;
            _session.Flush();
            _tx.Commit();
            _tx.Dispose();
            _tx = null;
        }
    }
}

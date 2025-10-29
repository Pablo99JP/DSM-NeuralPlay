using System;
using NHibernate;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ISession _session;
        private ITransaction? _tx;
        private bool _disposed;

        public NHibernateUnitOfWork(ISession session)
        {
            _session = session;
            _tx = _session.BeginTransaction();
            _disposed = false;
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

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                if (_tx != null)
                {
                    try
                    {
                        if (_tx.IsActive)
                        {
                            // If not committed, rollback
                            _tx.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                        // Some ADO providers (SQLite in-memory lifecycle) may report no active transaction
                        // or throw when rolling back after commit/dispose. Swallow rollback exceptions here.
                    }
                    finally
                    {
                        try { _tx.Dispose(); } catch { }
                        _tx = null;
                    }
                }
            }
            finally
            {
                if (_session != null && _session.IsOpen)
                {
                    try { _session.Close(); } catch { }
                    try { _session.Dispose(); } catch { }
                }
                _disposed = true;
            }
        }
    }
}

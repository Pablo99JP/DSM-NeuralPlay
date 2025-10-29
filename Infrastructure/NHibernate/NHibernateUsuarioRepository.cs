using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    // Repository skeleton: implement using NHibernate session in future iteration.
    public class NHibernateUsuarioRepository : IUsuarioRepository
    {
        public Usuario? ReadById(long id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Usuario> ReadAll()
        {
            throw new NotImplementedException();
        }

        public void New(Usuario entity)
        {
            throw new NotImplementedException();
        }

        public void Modify(Usuario entity)
        {
            throw new NotImplementedException();
        }

        public void Destroy(long id)
        {
            throw new NotImplementedException();
        }

        public Usuario? ReadByNick(string nick)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Usuario> ReadFilter(string filter)
        {
            throw new NotImplementedException();
        }
    }
}

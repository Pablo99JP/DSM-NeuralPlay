using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateMiembroEquipoRepository : IMiembroEquipoRepository
    {
        private readonly ISession _session;

        public NHibernateMiembroEquipoRepository(ISession session)
        {
            _session = session;
        }

        public MiembroEquipo? ReadById(long id) => _session.Get<MiembroEquipo>(id);

        public IEnumerable<MiembroEquipo> ReadAll()
        {
            var q = _session.CreateQuery("from MiembroEquipo");
            return q.List<MiembroEquipo>();
        }

        public IEnumerable<MiembroEquipo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from MiembroEquipo m where m.Usuario is not null and lower(m.Usuario.Nick) like :f");
            q.SetParameter("f", f);
            return q.List<MiembroEquipo>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<MiembroEquipo> BuscarMiembrosEquipoPorNickUsuario(string filtro) => ReadFilter(filtro);

        public void New(MiembroEquipo entity) => _session.Save(entity);
        public void Modify(MiembroEquipo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        public IEnumerable<Usuario> GetUsuariosByEquipo(long idEquipo)
        {
            var q = _session.CreateQuery("select distinct m.Usuario from MiembroEquipo m where m.Equipo is not null and m.Equipo.IdEquipo = :idEquipo and m.Usuario is not null");
            q.SetParameter("idEquipo", idEquipo);
            return q.List<Usuario>();
        }

        public IEnumerable<Equipo> GetEquiposByUsuario(long idUsuario)
        {
            var q = _session.CreateQuery("select distinct m.Equipo from MiembroEquipo m where m.Usuario is not null and m.Usuario.IdUsuario = :idUsuario and m.Equipo is not null and m.Estado = :estado");
            q.SetParameter("idUsuario", idUsuario);
            q.SetParameter("estado", ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);
            return q.List<Equipo>();
        }
    }
}

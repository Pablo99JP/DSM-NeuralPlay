using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class EquipoCEN
    {
        private readonly IRepository<Equipo> _repo;

        public EquipoCEN(IRepository<Equipo> repo)
        {
            _repo = repo;
        }

        public Equipo NewEquipo(string nombre, string? descripcion = null)
        {
            var e = new Equipo { Nombre = nombre, Descripcion = descripcion, FechaCreacion = System.DateTime.UtcNow };
            _repo.New(e);
            return e;
        }

        public Equipo? ReadOID_Equipo(long id) => _repo.ReadById(id);
        public IEnumerable<Equipo> ReadAll_Equipo() => _repo.ReadAll();
        public void ModifyEquipo(Equipo e) => _repo.Modify(e);
        public void DestroyEquipo(long id) => _repo.Destroy(id);
        public IEnumerable<Equipo> BuscarEquiposPorNombreODescripcion(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarEquiposPorNombreODescripcion");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<Equipo> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}

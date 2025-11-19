using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class VotoTorneoCEN
    {
        private readonly IRepository<VotoTorneo> _repo;

        public VotoTorneoCEN(IRepository<VotoTorneo> repo)
        {
            _repo = repo;
        }

        public VotoTorneo NewVotoTorneo(bool valor, Usuario votante, PropuestaTorneo propuesta)
        {
            var v = new VotoTorneo { Valor = valor, FechaVoto = System.DateTime.UtcNow, Votante = votante, Propuesta = propuesta };
            _repo.New(v);
            return v;
        }

        public VotoTorneo? ReadOID_VotoTorneo(long id) => _repo.ReadById(id);
        public IEnumerable<VotoTorneo> ReadAll_VotoTorneo() => _repo.ReadAll();
        public void ModifyVotoTorneo(VotoTorneo v) => _repo.Modify(v);
        public void DestroyVotoTorneo(long id) => _repo.Destroy(id);
        public IEnumerable<VotoTorneo> BuscarVotosTorneoPorNombreTorneo(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarVotosTorneoPorNombreTorneo");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<VotoTorneo> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}

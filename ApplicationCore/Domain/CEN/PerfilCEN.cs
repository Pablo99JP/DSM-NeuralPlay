using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class PerfilCEN
    {
        private readonly IRepository<Perfil> _repo;

        public PerfilCEN(IRepository<Perfil> repo)
        {
            _repo = repo;
        }

        public Perfil NewPerfil(Usuario usuario)
        {
            var p = new Perfil { Usuario = usuario, VisibilidadPerfil = ApplicationCore.Domain.Enums.Visibilidad.PUBLICO, VisibilidadActividad = ApplicationCore.Domain.Enums.Visibilidad.PUBLICO };
            _repo.New(p);
            return p;
        }

        public Perfil? ReadOID_Perfil(long id) => _repo.ReadById(id);
        public IEnumerable<Perfil> ReadAll_Perfil() => _repo.ReadAll();
        public void ModifyPerfil(Perfil p) => _repo.Modify(p);
        public void DestroyPerfil(long id) => _repo.Destroy(id);
        public IEnumerable<Perfil> ReadFilter_Perfil(string filter) => _repo.ReadFilter(filter);
    }
}

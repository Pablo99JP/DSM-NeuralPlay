using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System;

namespace ApplicationCore.Domain.CP
{
    public class ActualizarPerfilCP
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PerfilCEN _perfilCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly IRepository<Perfil> _perfilRepository;

        public ActualizarPerfilCP(IUnitOfWork unitOfWork, PerfilCEN perfilCEN, UsuarioCEN usuarioCEN, IRepository<Perfil> perfilRepository)
        {
            _unitOfWork = unitOfWork;
            _perfilCEN = perfilCEN;
            _usuarioCEN = usuarioCEN;
            _perfilRepository = perfilRepository;
        }

        public void Ejecutar(long idPerfil, string nuevoNick, string? nuevaDescripcion, string? nuevaFotoUrl)
        {
            // --- INICIO DE LA CORRECCIÓN ---
            // No es necesario iniciar la transacción manualmente.
            // El UnitOfWorkMiddleware ya la gestiona por nosotros.

            // 1. Cargar el perfil y su usuario asociado
            var perfil = _perfilRepository.ReadById(idPerfil);
            if (perfil == null || perfil.Usuario == null)
            {
                throw new InvalidOperationException("El perfil o el usuario asociado no existen.");
            }

            // 2. Actualizar las propiedades del Perfil
            perfil.Descripcion = nuevaDescripcion;
            perfil.FotoPerfilUrl = nuevaFotoUrl;
            _perfilCEN.ModifyPerfil(perfil);

            // 3. Actualizar las propiedades del Usuario (si han cambiado)
            if (perfil.Usuario.Nick != nuevoNick)
            {
                perfil.Usuario.Nick = nuevoNick;
                _usuarioCEN.ModifyUsuario(perfil.Usuario);
            }

            // 4. Guardar todos los cambios. El middleware se encargará del Commit/Rollback.
            _unitOfWork.SaveChanges();
            // --- FIN DE LA CORRECCIÓN ---
        }
    }
}
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Linq;

namespace ApplicationCore.Domain.CEN
{
    public class MiembroComunidadCEN
    {
        private readonly IMiembroComunidadRepository _repo;

        public MiembroComunidadCEN(IMiembroComunidadRepository repo)
        {
            _repo = repo;
        }

        public MiembroComunidad NewMiembroComunidad(Usuario usuario, Comunidad comunidad, ApplicationCore.Domain.Enums.RolComunidad rol)
        {
            // Verificar si el usuario fue expulsado anteriormente de esta comunidad
            var membresiaAnterior = _repo.ReadAll()
                .FirstOrDefault(m => m.Usuario.IdUsuario == usuario.IdUsuario 
                    && m.Comunidad.IdComunidad == comunidad.IdComunidad 
                    && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA);

            if (membresiaAnterior != null)
            {
                throw new System.InvalidOperationException("El usuario fue expulsado de esta comunidad y no puede volver a ingresar.");
            }

            // Si el rol es LIDER, verificar que no exista otro líder activo en la comunidad
            if (rol == ApplicationCore.Domain.Enums.RolComunidad.LIDER)
            {
                var liderExistente = _repo.ReadAll()
                    .Any(m => m.Comunidad.IdComunidad == comunidad.IdComunidad 
                        && m.Rol == ApplicationCore.Domain.Enums.RolComunidad.LIDER 
                        && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);

                if (liderExistente)
                {
                    throw new System.InvalidOperationException("Ya existe un líder en esta comunidad. No puede haber dos líderes.");
                }
            }

            // Verificar si ya existe una membresía activa
            var membresiaActiva = _repo.ReadAll()
                .FirstOrDefault(m => m.Usuario.IdUsuario == usuario.IdUsuario 
                    && m.Comunidad.IdComunidad == comunidad.IdComunidad 
                    && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);

            if (membresiaActiva != null)
            {
                throw new System.InvalidOperationException("El usuario ya es miembro activo de esta comunidad.");
            }

            var m = new MiembroComunidad { Usuario = usuario, Comunidad = comunidad, Rol = rol, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = System.DateTime.UtcNow };
            _repo.New(m);
            return m;
        }

        public MiembroComunidad? ReadOID_MiembroComunidad(long id) => _repo.ReadById(id);
        public System.Collections.Generic.IEnumerable<MiembroComunidad> ReadAll_MiembroComunidad() => _repo.ReadAll();
        public void ModifyMiembroComunidad(MiembroComunidad m) => _repo.Modify(m);
        public void DestroyMiembroComunidad(long id) => _repo.Destroy(id);

        // Custom operations
        public void Salir(MiembroComunidad miembro)
        {
            miembro.Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ABANDONADA;
            miembro.FechaBaja = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }

        public void Expulsar(MiembroComunidad miembro)
        {
            miembro.Estado = ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA;
            miembro.FechaBaja = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }

        public void CambiarRol(MiembroComunidad miembro, ApplicationCore.Domain.Enums.RolComunidad nuevoRol)
        {
            // Si el nuevo rol es LIDER, verificar que no exista otro líder activo en la comunidad
            if (nuevoRol == ApplicationCore.Domain.Enums.RolComunidad.LIDER)
            {
                var liderExistente = _repo.ReadAll()
                    .Any(m => m.Comunidad.IdComunidad == miembro.Comunidad.IdComunidad 
                        && m.IdMiembroComunidad != miembro.IdMiembroComunidad
                        && m.Rol == ApplicationCore.Domain.Enums.RolComunidad.LIDER 
                        && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);

                if (liderExistente)
                {
                    throw new System.InvalidOperationException("Ya existe un líder en esta comunidad. No puede haber dos líderes.");
                }
            }

            miembro.Rol = nuevoRol;
            _repo.Modify(miembro);
        }

        // Promocionar a Moderador
        public void PromocionarAModerador(MiembroComunidad miembro)
        {
            miembro.Rol = ApplicationCore.Domain.Enums.RolComunidad.MODERADOR;
            miembro.FechaAccion = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }

        // Actualizar la fecha de acción (última actividad / intervención administrativa)
        public void ActualizarFechaAccion(MiembroComunidad miembro)
        {
            miembro.FechaAccion = System.DateTime.UtcNow;
            _repo.Modify(miembro);
        }

        // ReadFilter custom: Selecciona todos los Usuarios que tengan una membresía de comunidad cuya comunidad coincida con el id
        public System.Collections.Generic.IEnumerable<ApplicationCore.Domain.EN.Usuario> ReadFilter_UsuariosByComunidadMembership(long idComunidad)
        {
            return _repo.GetUsuariosByComunidad(idComunidad);
        }
    }
}

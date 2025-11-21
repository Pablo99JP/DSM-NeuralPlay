using System;

namespace ApplicationCore.Domain.EN
{
    /// <summary>
    /// Entidad de dominio simplificada para el módulo de identidad.
    /// Nombre solicitado: UsuarioEN
    /// </summary>
    public class UsuarioEN
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }
    }
}

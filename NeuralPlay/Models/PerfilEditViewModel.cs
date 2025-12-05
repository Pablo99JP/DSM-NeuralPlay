using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NeuralPlay.Models
{
    public class PerfilEditViewModel
    {
        public long IdPerfil { get; set; }

        [Required(ErrorMessage = "El nick es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nick no puede tener m�s de 50 caracteres.")]
        public string NickUsuario { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripci�n no puede tener m�s de 1000 caracteres.")]
        public string? Descripcion { get; set; }

        // Propiedad para almacenar la ruta actual de la imagen (campo oculto)
        [StringLength(255, ErrorMessage = "La URL no puede tener m�s de 255 caracteres.")]
        public string? FotoPerfilUrl { get; set; }

        // Nueva propiedad para subir archivo de imagen
        [Display(Name = "Imagen de Perfil")]
        public IFormFile? ImagenArchivo { get; set; }
    }
}
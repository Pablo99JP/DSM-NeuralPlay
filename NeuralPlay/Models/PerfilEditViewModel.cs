using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    public class PerfilEditViewModel
    {
        public long IdPerfil { get; set; }

        [Required(ErrorMessage = "El nick es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nick no puede tener más de 50 caracteres.")]
        public string NickUsuario { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede tener más de 1000 caracteres.")]
        public string? Descripcion { get; set; }

        // --- INICIO DE LA CORRECCIÓN ---
        // Se elimina el atributo [Url] para no forzar la validación de la URL.
        [Display(Name = "URL del Avatar")]
        [StringLength(255, ErrorMessage = "La URL no puede tener más de 255 caracteres.")]
        public string? FotoPerfilUrl { get; set; }
        // --- FIN DE LA CORRECCIÓN ---
    }
}
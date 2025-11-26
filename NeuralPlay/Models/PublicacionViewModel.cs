using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    public class PublicacionViewModel
    {
        [ScaffoldColumn(false)]
        public long idPublicacion { get; set; }

        [Display(Prompt = "Publica algo...", Description = "Contenido de la publicacion", Name = "Contenido")]
        [Required(ErrorMessage = "El contenido de la publicacion es obligatorio")]
        [StringLength(1000, ErrorMessage = "El contenido no puede exceder los 1000 caracteres")]
        public string contenido { get; set; }  = string.Empty;

        [Display(Name = "Fecha de Publicacion")]
        public DateTime fechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Edicion")]
        public DateTime fechaEdicion { get; set; } = DateTime.Now;
    }
}

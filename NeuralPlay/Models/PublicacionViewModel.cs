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

        // Comunidad y autor
        public long? IdComunidad { get; set; }

        [Display(Name = "Comunidad")]
        public string? NombreComunidad { get; set; }

        public long? IdAutor { get; set; }

        [Display(Name = "Autor")]
        public string? NickAutor { get; set; }

        // Comentarios asociados a la publicación
        public IEnumerable<ComentarioViewModel> comentarios { get; set; } = new List<ComentarioViewModel>();

        // Likes
        public int LikeCount { get; set; }
        public bool LikedByUser { get; set; }
    }
}

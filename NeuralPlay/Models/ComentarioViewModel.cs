using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
 public class ComentarioViewModel
 {
 [ScaffoldColumn(false)]
 public long idComentario { get; set; }

 [ScaffoldColumn(false)]
 public long publicacionId { get; set; }

 [Display(Prompt = "Escribe un comentario...", Description = "Contenido del comentario", Name = "Comentario")]
 [Required(ErrorMessage = "El contenido del comentario es obligatorio")]
 [StringLength(500, ErrorMessage = "El comentario no puede exceder los500 caracteres")]
 public string contenido { get; set; } = string.Empty;

 [Display(Name = "Fecha de Creación")]
 public DateTime fechaCreacion { get; set; } = DateTime.Now;

 [Display(Name = "Fecha de Edición")]
 public DateTime fechaEdicion { get; set; } = DateTime.Now;

 // Read-only helper for views
 public string? autorNick { get; set; }

 // Id del autor para comprobaciones de permisos
 public long autorId { get; set; }
 }
}

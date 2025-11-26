using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    public class PerfilViewModel
    {
        public long IdPerfil { get; set; }

        public long IdUsuario { get; set; }

        [Display(Name = "Usuario")]
        public string? NickUsuario { get; set; }

        [Display(Name = "Biografía")]
        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Display(Name = "URL del Avatar")]
        [Url]
        public string? Avatar { get; set; }

        public IList<JuegoViewModel> Juegos { get; set; } = new List<JuegoViewModel>();
    }
}
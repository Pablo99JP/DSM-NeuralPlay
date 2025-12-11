using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NeuralPlay.Models
{
    public class ComunidadViewModel
    {
        public long IdComunidad { get; set; }

        [Required(ErrorMessage = "El nombre de la comunidad es obligatorio.")]
        [Display(Name = "Nombre de la Comunidad")]
        public string? Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Es pública")]

    public bool EsPublica { get; set; }

    // Ruta de la imagen asociada a la comunidad
    public string? ImagenUrl { get; set; }

    // Archivo de imagen para subir
    [Display(Name = "Imagen de la comunidad")]
    public IFormFile? ImagenArchivo { get; set; }

        [Display(Name = "Propietario")]
        public string? Propietario { get; set; }

        // Indica si el usuario actual es miembro de la comunidad
        public bool IsMember { get; set; }
    }
}

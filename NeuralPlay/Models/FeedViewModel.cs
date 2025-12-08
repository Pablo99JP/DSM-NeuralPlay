using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    /// <summary>
    /// ViewModel para mostrar el Feed del perfil de un usuario con 4 secciones:
    /// 1. Actividad (Publicaciones, Comentarios y Me Gusta)
    /// 2. Publicaciones del Usuario
    /// 3. Me Gusta del Usuario
    /// 4. Comentarios del Usuario (publicaciones donde ha comentado)
    /// </summary>
    public class FeedViewModel
    {
        [Display(Name = "ID Perfil")]
        public long IdPerfil { get; set; }

        [Display(Name = "ID Usuario")]
        public long IdUsuario { get; set; }

        [Display(Name = "Usuario")]
        public string? NickUsuario { get; set; }

        [Display(Name = "Biografía")]
        public string? Descripcion { get; set; }

        [Display(Name = "Avatar")]
        public string? Avatar { get; set; }

        // ============================================
        // SECCIÓN 1: ACTIVIDAD (Últimas acciones)
        // ============================================
        [Display(Name = "Actividad Reciente")]
        public IList<ActividadViewModel> Actividades { get; set; } = new List<ActividadViewModel>();

        // ============================================
        // SECCIÓN 2: PUBLICACIONES DEL USUARIO
        // ============================================
        [Display(Name = "Publicaciones")]
        public IList<PublicacionViewModel> Publicaciones { get; set; } = new List<PublicacionViewModel>();

        // ============================================
        // SECCIÓN 3: ME GUSTA DEL USUARIO
        // ============================================
        [Display(Name = "Me Gusta")]
        public IList<PublicacionViewModel> MeGustas { get; set; } = new List<PublicacionViewModel>();

        // ============================================
        // SECCIÓN 4: COMENTARIOS DEL USUARIO
        // ============================================
        [Display(Name = "Comentarios")]
        public IList<ComentarioEnPublicacionViewModel> Comentarios { get; set; } = new List<ComentarioEnPublicacionViewModel>();
    }

    /// <summary>
    /// Representa una actividad en el feed (Publicación, Comentario o Me Gusta)
    /// </summary>
    public class ActividadViewModel
    {
        [Display(Name = "Tipo de Actividad")]
        public TipoActividad Tipo { get; set; }

        [Display(Name = "Fecha")]
        public System.DateTime Fecha { get; set; }

        // Datos de la publicación (en caso de que sea comentario o me gusta sobre publicación)
        public long? IdPublicacion { get; set; }
        public string? ContenidoPublicacion { get; set; }
        public string? NombreComunidad { get; set; }

        // Datos del comentario (si es me gusta sobre comentario)
        public long? IdComentario { get; set; }
        public string? ContenidoComentario { get; set; }

        // Descripción genérica de la actividad
        public string? Descripcion { get; set; }
    }

    /// <summary>
    /// Enum para identificar el tipo de actividad
    /// </summary>
    public enum TipoActividad
    {
        [Display(Name = "Publicación")]
        Publicacion,

        [Display(Name = "Comentario")]
        Comentario,

        [Display(Name = "Me Gusta en Publicación")]
        MeGustanPublicacion,

        [Display(Name = "Me Gusta en Comentario")]
        MeGustanComentario
    }

    /// <summary>
    /// Vista de un comentario dentro de una publicación
    /// </summary>
    public class ComentarioEnPublicacionViewModel
    {
        [Display(Name = "ID Comentario")]
        public long IdComentario { get; set; }

        [Display(Name = "Contenido del Comentario")]
        public string? ContenidoComentario { get; set; }

        [Display(Name = "Fecha del Comentario")]
        public System.DateTime FechaComentario { get; set; }

        // Datos de la publicación relacionada
        [Display(Name = "ID Publicación")]
        public long IdPublicacion { get; set; }

        [Display(Name = "Contenido de la Publicación")]
        public string? ContenidoPublicacion { get; set; }

        [Display(Name = "Autor de la Publicación")]
        public string? NickAutorPublicacion { get; set; }

        [Display(Name = "Avatar del Autor de la Publicación")]
        public string? AvatarAutorPublicacion { get; set; }

        [Display(Name = "Fecha de Publicación")]
        public System.DateTime FechaPublicacion { get; set; }

        [Display(Name = "Comunidad")]
        public string? NombreComunidad { get; set; }

        // Likes en el comentario
        public int LikeCount { get; set; }
    }
}

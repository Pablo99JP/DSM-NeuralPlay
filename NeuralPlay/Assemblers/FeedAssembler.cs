using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using NeuralPlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralPlay.Assemblers
{
    /// <summary>
    /// Assembler para convertir datos del Feed desde EN a ViewModel
    /// </summary>
    public static class FeedAssembler
    {
        /// <summary>
        /// Convierte una Actividad (Publicación, Comentario o Reacción) a ActividadViewModel
        /// </summary>
        public static ActividadViewModel? ConvertPublicacionToActividad(Publicacion publicacion)
        {
            if (publicacion == null) return null;

            return new ActividadViewModel
            {
                Tipo = TipoActividad.Publicacion,
                Fecha = publicacion.FechaCreacion,
                IdPublicacion = publicacion.IdPublicacion,
                ContenidoPublicacion = publicacion.Contenido,
                NombreComunidad = publicacion.Comunidad?.Nombre,
                Descripcion = $"Publicó: \"{publicacion.Contenido.Substring(0, Math.Min(50, publicacion.Contenido.Length))}...\""
            };
        }

        /// <summary>
        /// Convierte un Comentario a ActividadViewModel
        /// </summary>
        public static ActividadViewModel? ConvertComentarioToActividad(Comentario comentario)
        {
            if (comentario == null) return null;

            return new ActividadViewModel
            {
                Tipo = TipoActividad.Comentario,
                Fecha = comentario.FechaCreacion,
                IdPublicacion = comentario.Publicacion?.IdPublicacion,
                ContenidoPublicacion = comentario.Publicacion?.Contenido,
                IdComentario = comentario.IdComentario,
                ContenidoComentario = comentario.Contenido,
                NombreComunidad = comentario.Publicacion?.Comunidad?.Nombre,
                Descripcion = $"Comentó: \"{comentario.Contenido.Substring(0, Math.Min(50, comentario.Contenido.Length))}...\""
            };
        }

        /// <summary>
        /// Convierte una Reacción a ActividadViewModel
        /// </summary>
        public static ActividadViewModel? ConvertReaccionToActividad(Reaccion reaccion)
        {
            if (reaccion == null) return null;

            if (reaccion.Publicacion != null)
            {
                return new ActividadViewModel
                {
                    Tipo = TipoActividad.MeGustanPublicacion,
                    Fecha = reaccion.FechaCreacion,
                    IdPublicacion = reaccion.Publicacion.IdPublicacion,
                    ContenidoPublicacion = reaccion.Publicacion.Contenido,
                    NombreComunidad = reaccion.Publicacion.Comunidad?.Nombre,
                    Descripcion = $"Le gustó una publicación"
                };
            }
            else if (reaccion.Comentario != null)
            {
                return new ActividadViewModel
                {
                    Tipo = TipoActividad.MeGustanComentario,
                    Fecha = reaccion.FechaCreacion,
                    IdComentario = reaccion.Comentario.IdComentario,
                    ContenidoComentario = reaccion.Comentario.Contenido,
                    IdPublicacion = reaccion.Comentario.Publicacion?.IdPublicacion,
                    ContenidoPublicacion = reaccion.Comentario.Publicacion?.Contenido,
                    NombreComunidad = reaccion.Comentario.Publicacion?.Comunidad?.Nombre,
                    Descripcion = $"Le gustó un comentario"
                };
            }

            return null;
        }

        /// <summary>
        /// Convierte un Comentario en Publicación a ComentarioEnPublicacionViewModel
        /// </summary>
        public static ComentarioEnPublicacionViewModel? ConvertComentarioEnPublicacion(Comentario comentario, int likeCount = 0)
        {
            if (comentario == null) return null;

            return new ComentarioEnPublicacionViewModel
            {
                IdComentario = comentario.IdComentario,
                ContenidoComentario = comentario.Contenido,
                FechaComentario = comentario.FechaCreacion,
                IdPublicacion = comentario.Publicacion?.IdPublicacion ?? 0,
                ContenidoPublicacion = comentario.Publicacion?.Contenido,
                NickAutorPublicacion = comentario.Publicacion?.Autor?.Nick,
                AvatarAutorPublicacion = comentario.Publicacion?.Autor?.Perfil?.FotoPerfilUrl,
                FechaPublicacion = comentario.Publicacion?.FechaCreacion ?? DateTime.MinValue,
                NombreComunidad = comentario.Publicacion?.Comunidad?.Nombre,
                LikeCount = likeCount
            };
        }
    }
}
